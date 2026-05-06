using Godot;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace GameWizard.Engine.Config.Yaml;

public class YamlConfigLoader : IConfigLoader
{
    public IDeserializer Deserializer { get; set; } = new DeserializerBuilder()
        .WithNamingConvention(HyphenatedNamingConvention.Instance)
        .WithEnumNamingConvention(HyphenatedNamingConvention.Instance)

        .WithTypeConverter(new ConditionYamlConverter())
        .WithTypeConverter(new GameEdgeYamlConverter())
        .WithTypeConverter(new GaneStateUpdateYamlConverter())
        .WithTypeConverter(new Vector2YamlConverter())

        .Build();

    public T Load<T>(string path)
    {
        var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        var configText = file.GetAsText();
        return Deserializer.Deserialize<T>(configText);
    }
}