using System;
using System.Collections.Generic;
using GameWizard.Engine.Schema.Game;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace GameWizard.Engine.Config.Yaml;

public class DbEntryYamlConverter : IYamlTypeConverter
{
    public bool Accepts(Type type) => type == typeof(DbEntry);

    public object ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        parser.Consume<MappingStart>();

        var result = new DbEntry();

        while (parser.Current is not MappingEnd)
        {
            var key = parser.Consume<Scalar>().Value;
            if (parser.Current is Scalar)
                result.SimpleFields[key] = parser.Consume<Scalar>().Value;
            else
            {
                var list = new List<string>();
                parser.Consume<SequenceStart>();
                while (parser.Current is not SequenceEnd)
                    list.Add(parser.Consume<Scalar>().Value);
                parser.Consume<SequenceEnd>();
                result.CollectionFields[key] = list;
            }
        }

        parser.Consume<MappingEnd>();

        return result;
    }

    public void WriteYaml(IEmitter emitter, object value, Type type, ObjectSerializer serializer)
    {
        throw new NotImplementedException();
    }
}