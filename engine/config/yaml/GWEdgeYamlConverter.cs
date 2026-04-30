using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using GameWizard.Engine.Schema;
using GameWizard.Engine.Util;
using Godot;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace GameWizard.Engine.Config.Yaml;

public class GWEdgeYamlConverter : IYamlTypeConverter
{
    private static readonly string SimpleEdge = @"^([a-zA-Z0-9.-]+) => ([a-zA-Z0-9-]+\.?[a-zA-Z0-9-]+)$";
    private static readonly string FullEdge = @"^([a-zA-Z0-9.-]+) => ([a-zA-Z0-9-]+\.?[a-zA-Z0-9-]+) => ((?:ref://parent|[a-zA-Z0-9.-]+))$";
    private static readonly string SimpleWhenFlagIn = @"^when flag ([a-zA-Z0-9.-]+) in \[([a-zA-Z0-9.,-]+)\]$";
    private static readonly string FullWhenFlagIn = @"^when flag ([a-zA-Z0-9.-]+) in \[([a-zA-Z0-9.,-]+)\] => ((?:ref://parent|[a-zA-Z0-9.-]+))$";
    private static readonly string Else = @"^else => ([a-zA-Z0-9.-]+)$";
    private static readonly string SetFlag = @"^set flag ([a-zA-Z0-9.-]+) to ([a-zA-Z0-9.-]+)$";

    private enum ParsingState
    {
        Uninitialized,
        DefiningCondition,
        DefiningEffects,
    }
    private ParsingState CurrentParsingState { get; set; }

    public bool Accepts(Type type) => type == typeof(IList<GWEdge>);

    public object ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        var result = new List<GWEdge>();

        parser.Consume<SequenceStart>();

        while (parser.Current is not SequenceEnd)
        {
            var raw = parser.Consume<Scalar>();
            CurrentParsingState = ParsingState.Uninitialized;

            var lines = raw.Value.Split(";")
                .WhereNot(string.IsNullOrEmpty)
                .Select(s => s.Trim())
                .ToList();

            var edge = new GWEdge();
            foreach (var line in lines)
            {
                var edgeReady = ParseLine(edge, line);

                if (edgeReady)
                {
                    result.Add(edge);
                    edge = new GWEdge();
                    Initialize(edge, lines[0]);
                    CurrentParsingState = ParsingState.DefiningCondition;
                    ParseLine(edge, line);
                }
            }

            result.Add(edge);
        }

        parser.Consume<SequenceEnd>();

        return result;
    }

    private bool ParseLine(GWEdge edge, string line)
    {
        switch (CurrentParsingState)
        {
            case ParsingState.Uninitialized:
                CurrentParsingState = Initialize(edge, line);
                break;
            case ParsingState.DefiningCondition:
                if (line.StartsWith("when ")) CurrentParsingState = ParseCondition(edge, line);
                else if (line.StartsWith("else ")) CurrentParsingState = ParseElse(edge, line);
                else throw new NotImplementedException();
                break;
            case ParsingState.DefiningEffects:
                if (line.StartsWith("when ") || line.StartsWith("else ")) return true;
                CurrentParsingState = ParseEffect(edge, line);
                break;
        }

        return false;
    }

    public void WriteYaml(IEmitter emitter, object value, Type type, ObjectSerializer serializer)
    {
        throw new NotImplementedException();
    }

    private ParsingState Initialize(GWEdge edge, string line)
    {
        var nextState = ParsingState.DefiningCondition;

        var match = Regex.Match(line, SimpleEdge);
        if (!match.Success)
        {
            match = Regex.Match(line, FullEdge);
            nextState = ParsingState.DefiningEffects;
            edge.Destination = match.Groups[3].Value;
        }

        if (!match.Success)
        {
            GD.PushError($"unable to parse {line}");
            throw new NotImplementedException();
        }

        var eventType = match.Groups[2].Value;
        var eventId = "ref://any";
        if (eventType.Contains("."))
        {
            var eventParts = eventType.Split(".");
            eventType = eventParts[0];
            eventId = eventParts[1];
        }

        edge.Source = match.Groups[1].Value;
        edge.Event = eventType;
        edge.EventId = eventId;

        return nextState;
    }

    private ParsingState ParseCondition(GWEdge edge, string line)
    {
        var nextState = ParsingState.DefiningCondition;

        var match = Regex.Match(line, SimpleWhenFlagIn);
        if (!match.Success)
        {
            match = Regex.Match(line, FullWhenFlagIn);
            nextState = ParsingState.DefiningEffects;
            edge.Destination = match.Groups[3].Value;
        }

        edge.Conditions.Add(new GWCondition
        {
            Predicate = GWStatePredicate.FlagIn,
            Target = match.Groups[1].Value,
            Set = match.Groups[2].Value.Split(","),
        });

        return nextState;
    }

    private ParsingState ParseElse(GWEdge edge, string line)
    {
        var match = Regex.Match(line, Else);

        edge.Destination = match.Groups[1].Value;

        return ParsingState.DefiningEffects;
    }

    private ParsingState ParseEffect(GWEdge edge, string line)
    {
        var match = Regex.Match(line, SetFlag);

        edge.Changes.Add(new GWStateChange
        {
            Type = GWStateChangeType.SetFlag,
            Target = match.Groups[1].Value,
            Value = match.Groups[2].Value,
        });

        return ParsingState.DefiningEffects;
    }
}