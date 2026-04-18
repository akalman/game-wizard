using Godot;
using Prototypes.SceneModules.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using Prototypes.SceneModules.SceneTemplates;

namespace Prototypes.SceneModules;

public partial class SceneModulesGameController : Node2D
{

	private IDictionary<string, GWModuleScene> Templates => new Dictionary<string, GWModuleScene>()
	{
		{
			"core.dialog-cutscene", new GWModuleScene
			{
				ModuleSceneId = "dialog-cutscene",
				ModuleScenePath = "res://prototypes/scene-modules/scene-templates/dialog-cutscene.tscn",
				ModuleSceneCommands = [ "advance", "exit" ],
			}
		},
		{
			"core.landmark-overworld", new GWModuleScene
			{
				ModuleSceneId = "landmark-overworld",
				ModuleScenePath = "res://prototypes/scene-modules/scene-templates/landmark-overworld.tscn",
				ModuleSceneCommands = [ ],
			}
		},
	};

	private IDictionary<string, GWGameState> Scenes => new Dictionary<string, GWGameState>
	{
		{
			"world-map", new GWGameState
			{
				StateId = "world-map",
				Template = "core.landmark-overworld",
				ConfigPath = "world-map.config-path",
			}
		},
		{
			"repeatable-dialog-cutscene", new GWGameState
			{
				StateId = "repeatable-dialog-cutscene",
				Template = "core.dialog-cutscene",
				ConfigPath = "repeatable-dialog-cutscene.config-path",
                IsOverlay = true,
			}
		},
	};

	private string InitialScene => "world-map";
	private IDictionary<string, IDictionary<string, string>> Edges => new List<GWGameEdge>
		{
			new GWGameEdge { SourceScene = "world-map", EdgeName = "start-dialog", DestinationScene = "repeatable-dialog-cutscene" },
			new GWGameEdge { SourceScene = "world-map", EdgeName = "exit-game" },
			new GWGameEdge { SourceScene = "repeatable-dialog-cutscene", EdgeName = "end-dialog" },
		}
		.Aggregate(new Dictionary<string, IDictionary<string, string>>(), (a, b) =>
		{
			if (!a.ContainsKey(b.SourceScene)) a[b.SourceScene] = new Dictionary<string, string>();
			a[b.SourceScene][b.EdgeName] = b.DestinationScene;
			return a;
		});

	private IDictionary<string, Object> YamlProxy => new Dictionary<string, object>
	{
		{
			"world-map.config-path", new GWOverworld
			{
				OverworldId = "test-overworld-scene",
                Map = new GWOverworldMap
				{
					MapTexturePath = "res://prototypes/scene-modules/sprites/overworld-background.jpg",
                    ScalingType = GWMapScaling.FitWidth,
				},
                Landmarks = [
					new GWLandmark
					{
                        LandmarkId = "start-dialog-landmark",
                        Destination = "start-dialog",
                        Size = Vector2.One * 200,
                        TexturePath = "res://prototypes/scene-modules/sprites/landmark-a.png",
                        Offset = Vector2.Left * 0.6f + Vector2.Up * 0.4f
					},
					new GWLandmark
					{
                        LandmarkId = "exit-landmark",
                        Destination = "exit-game",
                        Size = Vector2.One * 200,
                        TexturePath = "res://prototypes/scene-modules/sprites/landmark-b.png",
                        Offset = Vector2.Right * 0.4f + Vector2.Down * 0.6f
					},
				]
			}
		},
		{
			"repeatable-dialog-cutscene.config-path", new GWDialog
			{
				DialogId = "test-dialog-scene",
				InitialFrameId = "initial-textbox",
                TerminalEdge = "end-dialog",
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
							LeftCharacterIds = ["char-a"],
							RightCharacterIds = ["char-b-flipped"],
							DialogText = "This is the the expected initial textbox content",
						},
						Edges =
						[
							new GWDialogEdge
							{
								Command = "advance",
								DestinationFrame = "add-in-character"
							},
							new GWDialogEdge
							{
								Command = "exit",
								DestinationFrame = "_exit"
							},
						],
					},
					new GWDialogFrame
					{
						FrameId = "add-in-character",
						State = new GWDialogFrameComponentState
						{
							LeftCharacterIds = ["char-a"],
							RightCharacterIds = ["char-b-flipped", "char-c-flipped"],
							DialogText =
								"This is the expected textbox content after adding an extra character to the right side of the screen.",
						},
						Edges =
						[
							new GWDialogEdge
							{
								Command = "advance",
								DestinationFrame = "move-character"
							},
							new GWDialogEdge
							{
								Command = "exit",
								DestinationFrame = "_exit"
							},
						],
					},
					new GWDialogFrame
					{
						FrameId = "move-character",
						State = new GWDialogFrameComponentState
						{
							LeftCharacterIds = ["char-a", "char-c"],
							RightCharacterIds = ["char-b-flipped"],
							DialogText =
								"This is the expected textbox content after moving the new character to the left side of the screen.",
						},
						Edges =
						[
							new GWDialogEdge
							{
								Command = "advance",
								DestinationFrame = "_exit"
							},
							new GWDialogEdge
							{
								Command = "exit",
								DestinationFrame = "_exit"
							},
						],
					},
				],
			}
		}
	};
    
	private IDictionary<string, SceneTemplateController> LoadedScenes { get; } = new Dictionary<string, SceneTemplateController>();
	private IList<string> SceneFocusStack { get; } = new List<string>();

	public override void _Ready()
	{
		LoadScene(InitialScene);
	}

	public override void _Process(double delta)
	{
		ProcessCommands();
	}
    
    public void TransitionScene(string edge)
    {
	    GD.PushWarning($"transitioning along edge {edge}");
	    var currentSceneId = SceneFocusStack[0];
        var targetSceneId = Edges[currentSceneId][edge];
        
	    GD.PushWarning($"checking to see if edge is terminal");
        if (targetSceneId == null || targetSceneId == string.Empty)
		{
	    	GD.PushWarning($"edge is terminal");
            (LoadedScenes[SceneFocusStack[0]] as Node).QueueFree();
			SceneFocusStack.RemoveAt(0);
            
            if (SceneFocusStack.Count <= 0)
			{
	    		GD.PushWarning($"game over");
				GetTree().Quit();
			}
            
            return;
		}
        
        var targetScene = Scenes[targetSceneId];
        if (!targetScene.IsOverlay)
		{
			foreach (var (_, controller) in LoadedScenes)
			{
				(controller as Node).QueueFree();
			}
			SceneFocusStack.Clear();
		}
        
        LoadScene(targetSceneId);
    }
    
    private void ProcessCommands()
	{
		foreach (var sceneId in SceneFocusStack)
		{
			var scene = Scenes[sceneId];
			var template = Templates[scene.Template];
            var controller = LoadedScenes[sceneId];
			foreach (var command in template.ModuleSceneCommands)
			{
                if (Input.IsActionJustPressed(command))
				{
					if (controller.ReceiveCommand(command))
					{
						return;
					}
				}
			}
		}
	}
    
    private void LoadScene(string sceneId)
	{
	    GD.PushWarning($"loading scene {sceneId}");
		var scene = Scenes[sceneId];
		var template = Templates[scene.Template];
        var packedScene = GD.Load<PackedScene>(template.ModuleScenePath);
		
		var instantiatedScene = packedScene.Instantiate() as SceneTemplateController;
		LoadedScenes[sceneId] = instantiatedScene;

		instantiatedScene.GameController = this;
		instantiatedScene.SetConfig(YamlProxy[scene.ConfigPath]);
		(instantiatedScene as CanvasItem).ZIndex = SceneFocusStack.Count;
        
        AddChild(instantiatedScene as Node);
        SceneFocusStack.Insert(0, sceneId);
	}
}
