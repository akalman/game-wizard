using GameWizard.Engine;
using GameWizard.Engine.Config;
using YamlDotNet.Serialization;

namespace GameWizard.Core.Menu;

public partial class MenuYamlPlugin : PluginController
{
	public override void RegisterDeserializer<T>(IConfigLoader<T> loader)
	{
		if (loader is IConfigLoader<IYamlTypeConverter> yaml)
		{
			yaml.RegisterDeserializer(new MenuActionYamlConverter());
		}
	}
}