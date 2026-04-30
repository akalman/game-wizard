using System;
using System.Text.RegularExpressions;
using Godot;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace GameWizard.Engine.Config.Yaml;

public class VectorYamlConverter : IYamlTypeConverter
{
    public bool Accepts(Type type) => type == typeof(Vector2);

    public object ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        var raw = parser.Consume<Scalar>();
        var parts = Regex.Match(raw.Value, @"^\s*\(\s*(-?\d+\.?\d*)\s*(-?\d+\.?\d*)\s*\)\s*$");
        return new Vector2(float.Parse(parts.Groups[1].Value), float.Parse(parts.Groups[2].Value));
    }

    public void WriteYaml(IEmitter emitter, object value, Type type, ObjectSerializer serializer)
    {
        var vector = value is Vector2 ? (Vector2)value : default;
        emitter.Emit(new Scalar($"( {vector.X} {vector.Y} )"));
    }
}