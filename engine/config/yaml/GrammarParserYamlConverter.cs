using System;
using System.Collections.Generic;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace GameWizard.Engine.Config.Yaml;

public class GrammarParserYamlConverter<T>(GrammarParser<T> grammarParser) : IYamlTypeConverter
{
    private GrammarParser<T> GrammarParser { get; } = grammarParser;

    public bool Accepts(Type type) => type == typeof(T);

    public object ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        return parser.Current switch
        {
            Scalar => ReadYamlString(parser),
            MappingStart => ReadYamlMap(parser),
            _ => throw new GameWizardInternalException()
        };
    }

    private T ReadYamlString(IParser parser)
    {
        return GrammarParser.Parse(new Dictionary<string, string> { [string.Empty] = parser.Consume<Scalar>().Value });
    }

    private T ReadYamlMap(IParser parser)
    {
        var map = new Dictionary<string, string>();

        parser.Consume<MappingStart>();

        while (parser.Current is not MappingEnd)
            map[parser.Consume<Scalar>().Value] = parser.Consume<Scalar>().Value;

        parser.Consume<MappingEnd>();

        return GrammarParser.Parse(map);
    }

    public void WriteYaml(IEmitter emitter, object value, Type type, ObjectSerializer serializer)
    {
        throw new NotImplementedException();
    }

    public static GrammarParserYamlConverter<T> Create(IList<GrammarTypeMapper<T>> mappers) => new(new GrammarParser<T>(mappers));
}