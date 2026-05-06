using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using GameWizard.Engine.Schema.Logic;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace GameWizard.Engine.Config.Yaml;

public class ConditionYamlConverter : IYamlTypeConverter
{
    private const string TermPattern = @"([.\w,-]+)";

    private static readonly IDictionary<ConditionType, string> Templates = new Dictionary<ConditionType, string>
    {
        { ConditionType.FlagIn, @"flag {0} in \[{1}\]" },
    };

    public bool Accepts(Type type) => type == typeof(Condition);

    public object ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        var raw = parser.Consume<Scalar>();

        foreach (var (updateType, template) in Templates)
        {
            var match = Regex.Match(raw.Value, $"^{string.Format(template, TermPattern, TermPattern)}$");
            if (match.Success)
            {
                return new Condition
                {
                    Type = updateType,
                    Target = match.Groups[1].Value,
                    ExpectedMembership = match.Groups[2].Success ? [ match.Groups[2].Value ] : new List<string>(),
                };
            }
        }

        throw new ConfigLoadingException($"Invalid condition format: {raw.Value}");
    }

    public void WriteYaml(IEmitter emitter, object value, Type type, ObjectSerializer serializer)
    {
        var update = value as Condition;
        if (update is null)
            throw new GameWizardInternalException(
                $"Tried to serialize {type.AssemblyQualifiedName} as a condition.");

        var str = string.Format(Templates[update.Type], update.Target, update.ExpectedMembership);

        emitter.Emit(new Scalar(str));
    }
}