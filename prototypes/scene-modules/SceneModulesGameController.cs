using Godot;
using Prototypes.SceneModules.DataModel;
using System;
using System.Collections.Generic;
using Prototypes.SceneModules.SceneTemplates;

namespace Prototypes.SceneModules;

public partial class SceneModulesGameController : Node2D
{
	private static IDictionary<string, GWModule> ModulesRegistry => new Dictionary<string, GWModule>
	{
		{ "core", new GWModule
		{
            ModuleId = "core",
			Scenes = new[]
			{
				new GWModuleScene
				{
					ModuleSceneId = "dialog-cutscene",
					ModuleScenePath = "res://prototypes/scene-modules/scene-templates/dialog-cutscene.tscn",
					ModuleSceneStructName = typeof(GWDialog).AssemblyQualifiedName,
                    ModuleSceneCommands = typeof(GWDialogCommands).AssemblyQualifiedName,
				}
			}
		} },
	};

	private IDictionary<string, PackedScene> SceneTemplateCache { get; set; } = new Dictionary<string, PackedScene>();
	private IDictionary<string, Node> LoadedScenes { get; set; } = new Dictionary<string, Node>();
	private IList<string> SceneFocusStack { get; set; } = new List<string>();

	public override void _Ready()
	{
		var initialScene = ModulesRegistry["core"].Scenes[0];
		var packedScene = GD.Load<PackedScene>(initialScene.ModuleScenePath);
		var instantiatedScene = packedScene.Instantiate();
		var fqsceneid = $"core.{initialScene.ModuleSceneId}";
		LoadedScenes[fqsceneid] = instantiatedScene;

		(instantiatedScene as DialogCutsceneController).GameController = this;
		(instantiatedScene as DialogCutsceneController).Config = new GWDialog
		{
            DialogId = "test-dialog-scene",
            InitialFrameId = "initial-textbox",
			Characters =
			[
				new GWDialogCharacter
				{
					CharacterId = "char-a",
                    SpritePath = "res://prototypes/scene-modules/sprites/char-a-profile.png",
				},
				new GWDialogCharacter
				{
					CharacterId = "char-b",
                    SpritePath = "res://prototypes/scene-modules/sprites/char-b-profile.png",
				},
				new GWDialogCharacter
				{
					CharacterId = "char-b-flipped",
                    SpritePath = "res://prototypes/scene-modules/sprites/char-b-profile.png",
				},
				new GWDialogCharacter
				{
					CharacterId = "char-c",
                    SpritePath = "res://prototypes/scene-modules/sprites/char-c-profile.png",
				},
				new GWDialogCharacter
				{
					CharacterId = "char-c-flipped",
                    SpritePath = "res://prototypes/scene-modules/sprites/char-c-profile.png",
				},
			],
            Frames =
			[
				new GWDialogFrame
				{
					FrameId = "initial-textbox",
                    State = new GWDialogFrameComponentState
					{
						LeftCharacterIds = [ "char-a" ],
                        RightCharacterIds = [ "char-b-flipped" ],
                        DialogText = "This is the the expected initial textbox content",
					},
                    Edges =
					[
                        new GWDialogEdge
                        {
                            Command = GWDialogCommands.Advance,
                            DestinationFrame = "add-in-character"
                        },
                        new GWDialogEdge
                        {
                            Command = GWDialogCommands.Exit,
                            DestinationFrame = "_exit"
                        },
					],
				},
				new GWDialogFrame
				{
					FrameId = "add-in-character",
                    State = new GWDialogFrameComponentState
					{
						LeftCharacterIds = [ "char-a" ],
                        RightCharacterIds = [ "char-b-flipped", "char-c-flipped" ],
                        DialogText = "This is the expected textbox content after adding an extra character to the right side of the screen.",
					},
                    Edges =
					[
                        new GWDialogEdge
                        {
                            Command = GWDialogCommands.Advance,
                            DestinationFrame = "move-character"
                        },
                        new GWDialogEdge
                        {
                            Command = GWDialogCommands.Exit,
                            DestinationFrame = "_exit"
                        },
					],
				},
				new GWDialogFrame
				{
					FrameId = "move-character",
                    State = new GWDialogFrameComponentState
					{
						LeftCharacterIds = [ "char-a", "char-c" ],
                        RightCharacterIds = [ "char-b-flipped" ],
                        DialogText = "This is the expected textbox content after moving the new character to the left side of the screen.",
					},
                    Edges =
					[
                        new GWDialogEdge
                        {
                            Command = GWDialogCommands.Advance,
                            DestinationFrame = "_exit"
                        },
                        new GWDialogEdge
                        {
                            Command = GWDialogCommands.Exit,
                            DestinationFrame = "_exit"
                        },
					],
				},
			],
		};
        
        AddChild(instantiatedScene);
        SceneFocusStack.Insert(0, fqsceneid);
	}

	public override void _Process(double delta)
	{
		var foundHandler = false;
		foreach (var fqsceneId in SceneFocusStack)
		{
			switch (fqsceneId)
			{
				case "core.dialog-cutscene":
					foreach (var commandName in Enum.GetNames<GWDialogCommands>())
					{
						if (Input.IsActionJustPressed($"{typeof(GWDialogCommands).FullName}.{commandName}"))
						{
							foundHandler = true;
							(LoadedScenes[fqsceneId] as DialogCutsceneController).ReceiveCommand(Enum.Parse<GWDialogCommands>(commandName));
						}
					}
                    if (foundHandler) return;
					break;
			}
		}
	}
}
