using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using GameWizard.Engine.Config;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace GameWizard.Core.DialogCutscene;

public class TransitionActionYamlConverter : IYamlTypeConverter
{
    private const string TermPattern = @"([.\w-]+)";

    private static readonly IDictionary<TransitionActionType, string> Templates = new Dictionary<TransitionActionType, string>
    {
        { TransitionActionType.End, "end" },
        { TransitionActionType.RollShot, "start {0}" },
        { TransitionActionType.SendAction, "action {0}" },
    };

    public bool Accepts(Type type) => type == typeof(TransitionAction);

    public object ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        var raw = parser.Consume<Scalar>();

        foreach (var (edgeType, template) in Templates)
        {
            var match = Regex.Match(raw.Value, $"^{string.Format(template, TermPattern)}$");
            if (match.Success)
            {
                return new TransitionAction
                {
                    Type = edgeType,
                    Destination = match.Groups[1].Value,
                };
            }
        }

        throw new ConfigLoadingException($"Invalid end action format: {raw.Value}");
    }

    public void WriteYaml(IEmitter emitter, object value, Type type, ObjectSerializer serializer)
    {
        throw new NotImplementedException();
    }
}