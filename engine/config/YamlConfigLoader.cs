using System;
using System.Collections.Generic;
using GameWizard.Core;
using GameWizard.Engine.Config.Yaml;
using Godot;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace GameWizard.Engine.Config;

public class YamlConfigLoader : IConfigLoader
{
    public IDeserializer Deserializer { get; set; } = new DeserializerBuilder()
        .WithNamingConvention(HyphenatedNamingConvention.Instance)
        .WithEnumNamingConvention(HyphenatedNamingConvention.Instance)
        .WithTypeConverter(new VectorYamlConverter())
        .WithTypeConverter(new GWEdgeYamlConverter())
        .WithTypeDiscriminatingNodeDeserializer(options =>
        {
            options.AddKeyValueTypeDiscriminator<GWDialogFrameUpdate>("type", new Dictionary<string, Type>
            {
                { "core.dialog-cutscene.set-text", typeof(GWSetTextUpdate) },
                { "core.dialog-cutscene.add-left-character", typeof(GWAddLeftCharacterUpdate) },
                { "core.dialog-cutscene.add-right-character", typeof(GWAddRightCharacterUpdate) },
                { "core.dialog-cutscene.remove-left-character", typeof(GWRemoveLeftCharacterUpdate) },
                { "core.dialog-cutscene.remove-right-character", typeof(GWRemoveRightCharacterUpdate) },
            });
        })
        .Build();

    public T Load<T>(string path)
    {
        GD.PushWarning($"loading yaml at {path}.");
        var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        var configText = file.GetAsText();
        return Deserializer.Deserialize<T>(configText);
    }
}