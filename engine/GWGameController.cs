using System;
using System.Collections.Generic;
using GameWizard.Engine.Schema;
using GameWizard.Engine.Util;
using Godot;

namespace GameWizard.Engine;

public partial class GWGameController : Node2D
{
    [Export(PropertyHint.File, "*.yaml")] public string GameConfig { get; set; }

    public IConfigLoader ConfigLoader { get; set; } = new YamlConfigLoader();

    public GWGame Game { get; set; }

    private IDictionary<string, GWTemplate> Templates { get; set; } = new Dictionary<string, GWTemplate>();
    private IDictionary<string, GWScreen> Screens { get; set; } = new Dictionary<string, GWScreen>();
    private IDictionary<string, string> Edges { get; set; } = new Dictionary<string, string>();

    private IDictionary<string, Node2D> LoadedScenes { get; } = new Dictionary<string, Node2D>();
    private IList<string> ScreenFocusStack { get; } = new List<string>();

    public override void _Ready()
    {
        InitializeGame();
        LoadScreen(Game.InitialScreen);
    }

    public override void _Process(double delta)
    {
        ProcessInputs();
    }

    public void TransitionScreen(string edgeType, string edge)
    {
        // validate the transition
        var currentScreenId = ScreenFocusStack[0];
        var currentScreen = Screens[currentScreenId];
        var currentTemplate = Templates[currentScreen.Template];

        if (!currentTemplate.EdgeTypes.Contains(edgeType))
        {
            GD.PushError(
                $"Received invalid edge type {edgeType} for current screen template {currentTemplate.TemplateId}.  Expected one of {currentTemplate.EdgeTypes}.");
            GetTree().Quit();
            return;
        }

        // if the edge is terminal, pop off stack and close game if stack now empty
        var targetScreenId = Edges[$"{currentScreenId}.{edgeType}.{edge}"];

        if (string.IsNullOrEmpty(targetScreenId))
        {
            LoadedScenes[ScreenFocusStack[0]].QueueFree();
            ScreenFocusStack.RemoveAt(0);

            if (ScreenFocusStack.Count <= 0) GetTree().Quit();
            return;
        }

        // if the target screen isn't an overlay, clear out current scenes
        var targetScreen = Screens[targetScreenId];

        if (!targetScreen.IsOverlay)
        {
            foreach (var (_, scene) in LoadedScenes)
            {
                scene.QueueFree();
            }

            LoadedScenes.Clear();
            ScreenFocusStack.Clear();
        }

        // load destination scene
        LoadScreen(targetScreenId);
    }

    private void ProcessInputs()
    {
        // don't bother processing if there are no screens loaded
        if (ScreenFocusStack.Count <= 0) return;

        foreach (var screenId in ScreenFocusStack)
        {
            var screen = Screens[screenId];
            var template = Templates[screen.Template];
            var scene = LoadedScenes[screenId];
            var controller = scene as GWTemplateController;

            foreach (var input in template.Inputs)
            {
                if (Input.IsActionJustPressed($"{template.TemplateId}.{input}"))
                {
                    if (controller.ReceiveInput(input))
                    {
                        return;
                    }
                }
            }

            // if this screen halts inputs don't process any further
            if (screen.HaltsInputs)
            {
                return;
            }
        }
    }

    private void LoadScreen(string screenId)
    {
        var screen = Screens[screenId];
        var template = Templates[screen.Template];
        var packedScene = GD.Load<PackedScene>(template.ScenePath);

        var scene = packedScene.Instantiate() as Node2D;
        if (scene == null)
        {
            GD.PushError($"Loaded scene for template {template.TemplateId} is not a Node2D scene.");
            GetTree().Quit();
            return;
        }

        var controller = scene as GWTemplateController;
        if (controller == null)
        {
            GD.PushError(
                $"Loaded scene for template {template.TemplateId} has no GWTemplateController implementation at the root.");
            GetTree().Quit();
            return;
        }

        LoadedScenes[screenId] = scene;

        controller.GameController = this;
        controller.SetConfig(ConfigLoader, screen.Config);
        scene.ZIndex = ScreenFocusStack.Count;

        AddChild(scene);
        ScreenFocusStack.Insert(0, screenId);
    }

    private void InitializeGame()
    {
        Game = ConfigLoader.Load<GWGame>(GameConfig);

        foreach (var moduleId in Game.Modules)
        {
            var module = ConfigLoader.Load<GWModule>($"res://{moduleId}/config.yaml");
            foreach (var template in module.Templates)
            {
                Templates[$"{module.ModuleId}.{template.TemplateId}"] = template;
            }
        }

        foreach (var screen in Game.Screens)
        {
            Screens[screen.ScreenId] = screen;
        }

        foreach (var edge in Game.Edges)
        {
            Edges[$"{edge.Source}.{edge.Type}.{edge.Edge}"] = edge.Destination;
        }
    }
}
