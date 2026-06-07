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
        var raw = parser.Consume<Scalar>();
        return GrammarParser.Parse(raw.Value);
    }

    public void WriteYaml(IEmitter emitter, object value, Type type, ObjectSerializer serializer)
    {
        throw new NotImplementedException();
    }

    public static GrammarParserYamlConverter<T> Create(IList<GrammarTypeMapper<T>> mappers) => new(new GrammarParser<T>(mappers));
}