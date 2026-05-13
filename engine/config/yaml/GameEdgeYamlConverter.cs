using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using GameWizard.Engine.Schema.Game;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace GameWizard.Engine.Config.Yaml;

public class GameEdgeYamlConverter : IYamlTypeConverter
{
    private const string TermPattern = @"([.\w-]+)";

    private static readonly IDictionary<EdgeType, string> Templates = new Dictionary<EdgeType, string>
    {
        { EdgeType.ToSibling, "{0} move to {1}" },
        { EdgeType.ToChild, "{0} spawn {1}" },
        { EdgeType.ToParent, "{0} ends" },
        { EdgeType.ToSelf, "{0} do nothing" },
        { EdgeType.Quit, "{0} quit"},
    };

    public bool Accepts(Type type) => type == typeof(Edge);

    public object ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        var raw = parser.Consume<Scalar>();

        foreach (var (edgeType, template) in Templates)
        {
            var match = Regex.Match(raw.Value, $"^{ string.Format(template, TermPattern, TermPattern) }$");
            if (match.Success)
            {
                var outputSegments = match.Groups[1].Value.Split(".");
                return new Edge
                {
                    OutputId = outputSegments[0],
                    OutputArg = outputSegments[1],
                    Type = edgeType,
                    Destination = match.Groups[2].Success ? match.Groups[2].Value : null,
                };
            }
        }

        throw new ConfigLoadingException($"Invalid edge format: {raw.Value}");
    }

    public void WriteYaml(IEmitter emitter, object value, Type type, ObjectSerializer serializer)
    {
        var edge = value as Edge;
        if (edge is null)
            throw new GameWizardInternalException($"Tried to serialize {type.AssemblyQualifiedName} as edge.");

        var str = string.Format(Templates[edge.Type], edge.OutputId, edge.Destination);

        emitter.Emit(new Scalar(str));
    }

}