using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using GameWizard.Engine.Schema.Game;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace GameWizard.Engine.Config.Yaml;

public class GaneStateUpdateYamlConverter : IYamlTypeConverter
{
    private const string TermPattern = @"([.\w-]+)";

    private static readonly IDictionary<StateUpdateType, string> Templates = new Dictionary<StateUpdateType, string>
    {
        { StateUpdateType.SetFlag, "set flag {0} to {0}" },

        { StateUpdateType.SetAttribute, "set attribute {0} to {0}" },
        { StateUpdateType.AddAttribute, "add {0} to attribute {0}" },
        { StateUpdateType.SubtractAttribute, "subtract {0} from attribute {0}" },

        { StateUpdateType.SetAmountInBag, "set item {0} in bag {0} to {0}" },
        { StateUpdateType.AddToBag, "add {0} of item {0} to bag {0}" },
        { StateUpdateType.RemoveFromBag, "remove {0} of item {0} from bag {0}" },
        { StateUpdateType.ClearBag, "clear bag {0}" },
    };

    public bool Accepts(Type type) => type == typeof(StateUpdate);

    public object ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        var raw = parser.Consume<Scalar>();

        foreach (var (updateType, template) in Templates)
        {
            var match = Regex.Match(raw.Value, $"^{string.Format(template, TermPattern)}$");
            if (match.Success)
            {
                return updateType switch
                {
                    // flag
                    StateUpdateType.SetFlag => new StateUpdate
                    {
                        Type = updateType,
                        StateName = match.Groups[1].Value,
                        String = match.Groups[2].Value,
                    },

                    // attribute
                    StateUpdateType.SetAttribute => new StateUpdate
                    {
                        Type = updateType,
                        StateName = match.Groups[1].Value,
                        Number = int.Parse(match.Groups[2].Value),
                    },
                    StateUpdateType.AddAttribute => new StateUpdate
                    {
                        Type = updateType,
                        StateName = match.Groups[2].Value,
                        Number = int.Parse(match.Groups[1].Value),
                    },
                    StateUpdateType.SubtractAttribute => new StateUpdate
                    {
                        Type = updateType,
                        StateName = match.Groups[2].Value,
                        Number = int.Parse(match.Groups[1].Value),
                    },

                    // bag
                    StateUpdateType.SetAmountInBag => new StateUpdate
                    {
                        Type = updateType,
                        StateName = match.Groups[2].Value,
                        BagItemName = match.Groups[1].Value,
                        Number = int.Parse(match.Groups[3].Value),
                    },
                    StateUpdateType.AddToBag => new StateUpdate
                    {
                        Type = updateType,
                        StateName = match.Groups[3].Value,
                        BagItemName = match.Groups[2].Value,
                        Number = int.Parse(match.Groups[1].Value),
                    },
                    StateUpdateType.RemoveFromBag => new StateUpdate
                    {
                        Type = updateType,
                        StateName = match.Groups[3].Value,
                        BagItemName = match.Groups[2].Value,
                        Number = int.Parse(match.Groups[1].Value),
                    },
                    StateUpdateType.ClearBag => new StateUpdate
                    {
                        Type = updateType,
                        StateName = match.Groups[3].Value,
                    },

                    // unexpected
                    _ => throw new GameWizardInternalException(),
                };
            }
        }

        throw new ConfigLoadingException($"Invalid update format: {raw.Value}");
    }

    public void WriteYaml(IEmitter emitter, object value, Type type, ObjectSerializer serializer)
    {
        var update = value as StateUpdate;
        if (update is null)
            throw new GameWizardInternalException($"Tried to serialize {type.AssemblyQualifiedName} as a game state update.");

        var str = string.Format(Templates[update.Type], update.StateName, update.String);

        emitter.Emit(new Scalar(str));
    }
}