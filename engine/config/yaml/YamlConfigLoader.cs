using System.Collections.Generic;
using Godot;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace GameWizard.Engine.Config.Yaml;

public class YamlConfigLoader : IConfigLoader<IYamlTypeConverter>
{
    public IDeserializer Deserializer { get; set; }

    private IList<IYamlTypeConverter> Deserializers { get; set; } = new List<IYamlTypeConverter>();

    public void RegisterDeserializer(IYamlTypeConverter deserializer)
    {
        Deserializers.Add(deserializer);
    }

    public T Load<T>(string path)
    {
        if (Deserializer is null)
            BuildDeserializer();

        GD.PushWarning($"Loading config at {path}.");
        var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        var configText = file.GetAsText();
        return Deserializer.Deserialize<T>(configText);
    }

    private void BuildDeserializer()
    {
        var builder = new DeserializerBuilder()
            .WithNamingConvention(HyphenatedNamingConvention.Instance)
            .WithEnumNamingConvention(HyphenatedNamingConvention.Instance);

        foreach (var deserializer in Deserializers)
            builder = builder.WithTypeConverter(deserializer);

        Deserializer = builder.Build();
    }
}