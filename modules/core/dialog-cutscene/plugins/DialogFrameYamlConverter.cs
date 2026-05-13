using System;
using System.Collections.Generic;
using GameWizard.Engine.Schema.Logic;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace GameWizard.Core.DialogCutscene;

public class DialogFrameYamlConverter : IYamlTypeConverter
{
    public bool Accepts(Type type) => type == typeof(IDialogFrame);

    public object ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        parser.Consume<MappingStart>();

        var properties = new Dictionary<string, string>();

        while (parser.Current is not MappingEnd)
        {
            properties[parser.Consume<Scalar>().Value] = parser.Consume<Scalar>().Value;
        }

        parser.Consume<MappingEnd>();

        if (!properties.TryGetValue("type", out var frameType))
            throw new InvalidDialogException($"Encountered dialog frame missing type parameter: {properties}");

        return frameType switch
        {
            "add-character" => new AddCharacterFrame
            {
                Character = properties["character"],
                Side = properties["side"] switch
                {
                    "left" => HorizontalDirection.Left,
                    "right" => HorizontalDirection.Right,
                    _ => throw new InvalidDialogException($"Encountered unrecognized side: {properties["side"]}")
                },
            },
            "remove-character" => new RemoveCharacterFrame
            {
                Character = properties["character"],
            },
            "set-text" => new SetTextFrame
            {
                Character = properties["character"],
                Text = properties["text"],
            },
            _ => throw new InvalidDialogException(),
        };
    }

    public void WriteYaml(IEmitter emitter, object value, Type type, ObjectSerializer serializer)
    {
        throw new NotImplementedException();
    }
}