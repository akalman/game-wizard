using System.Collections.Generic;
using GameWizard.Engine;
using GameWizard.Engine.Config;
using GameWizard.Engine.Config.Yaml;
using YamlDotNet.Serialization;

namespace GameWizard.Core.DialogCutscene;

public partial class DialogCutsceneYamlPlugin : PluginController
{
	public override void RegisterDeserializer<T>(IConfigLoader<T> loader)
	{
		if (loader is IConfigLoader<IYamlTypeConverter> yaml)
		{
			yaml.RegisterDeserializer(new TransitionActionYamlConverter());
			yaml.RegisterDeserializer(GrammarParserYamlConverter<IDialogFrame>.Create(
				new List<GrammarTypeMapper<IDialogFrame>>
				{
					new AddCharacterFrameParser(),
					new RemoveCharacterFrameParser(),
					new SetOutfitFrameParser(),
					new SetTextFrameParser(),
				}));
		}
	}
}
