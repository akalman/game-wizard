using GameWizard.Engine;
using GameWizard.Engine.Config;
using YamlDotNet.Serialization;

namespace GameWizard.Core.DialogCutscene;

public partial class DialogCutsceneYamlPlugin : PluginController
{
	public override void RegisterDeserializer<T>(IConfigLoader<T> loader)
	{
		if (loader is IConfigLoader<IYamlTypeConverter> yaml)
		{
			yaml.RegisterDeserializer(new ShotEndActionYamlConverter());
			yaml.RegisterDeserializer(new DialogFrameYamlConverter());
		}
	}
}
