using System;
using System.Collections.Generic;
using GameWizard.Engine.Schema;
using GameWizard.Engine.Util;
using Godot;

namespace GameWizard.Engine;

public partial class GWGameController : Node2D
{
    [Export(PropertyHint.File, "*.yaml")] public string GameConfig { get; set; }

    public YamlConfigLoader ConfigLoader { get; set; } = new YamlConfigLoader();

    public GWGame Game { get; set; }

    private IDictionary<string, GWTemplate> Templates { get; set; } = new Dictionary<string, GWTemplate>();
    private IDictionary<string, GWState> States { get; set; } = new Dictionary<string, GWState>();
    private IDictionary<string, string> Edges { get; set; } = new Dictionary<string, string>();

    private IDictionary<string, Node2D> LoadedStates { get; } = new Dictionary<string, Node2D>();
    private IList<string> StateFocusStack { get; } = new List<string>();

    public override void _Ready()
    {
        InitializeGame();
        LoadScene(Game.InitialState);
    }

    public override void _Process(double delta)
    {
        ProcessCommands();
    }

    public void TransitionScene(string edgeType, string edge)
    {
        // validate the transition
        var currentSceneId = StateFocusStack[0];
        var currentScene = States[currentSceneId];
        var currentTemplate = Templates[currentScene.Template];

        if (!currentTemplate.EdgeTypes.Contains(edgeType))
        {
            GD.PushError(
                $"Received invalid edge type {edgeType} for current scene template {currentTemplate.TemplateId}.  Expected one of {currentTemplate.EdgeTypes}.");
            GetTree().Quit();
            return;
        }

        // if the edge is terminal, pop off stack and close game if stack now empty
        var targetSceneId = Edges[$"{currentSceneId}.{edgeType}.{edge}"];

        if (string.IsNullOrEmpty(targetSceneId))
        {
            LoadedStates[StateFocusStack[0]].QueueFree();
            StateFocusStack.RemoveAt(0);

            if (StateFocusStack.Count <= 0) GetTree().Quit();
            return;
        }

        // if the target scene isn't an overlay, clear out current scenes
        var targetScene = States[targetSceneId];

        if (!targetScene.IsOverlay)
        {
            foreach (var (_, scene) in LoadedStates)
            {
                scene.QueueFree();
            }

            LoadedStates.Clear();
            StateFocusStack.Clear();
        }

        // load destination scene
        LoadScene(targetSceneId);
    }

    private void ProcessCommands()
    {
        // don't bother processing if there are no states loaded
        if (StateFocusStack.Count <= 0) return;

        foreach (var sceneId in StateFocusStack)
        {
            var state = States[sceneId];
            var template = Templates[state.Template];
            var scene = LoadedStates[sceneId];
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

            // if this state halts inputs don't process any further
            if (state.HaltsInputs)
            {
                return;
            }
        }
    }

    private void LoadScene(string stateId)
    {
        var state = States[stateId];
        var template = Templates[state.Template];
        var packedScene = GD.Load<PackedScene>(template.ScenePath);

        var instantiatedScene = packedScene.Instantiate() as Node2D;
        if (instantiatedScene == null)
        {
            GD.PushError($"Loaded scene for template {template.TemplateId} is not a Node2D scene.");
            GetTree().Quit();
            return;
        }

        var instantiatedController = instantiatedScene as GWTemplateController;
        if (instantiatedController == null)
        {
            GD.PushError(
                $"Loaded scene for template {template.TemplateId} has no GWTemplateController implementation at the root.");
            GetTree().Quit();
            return;
        }

        LoadedStates[stateId] = instantiatedScene;

        instantiatedController.GameController = this;
        instantiatedController.SetConfig(ConfigLoader, state.Config);
        instantiatedScene.ZIndex = StateFocusStack.Count;

        AddChild(instantiatedScene);
        StateFocusStack.Insert(0, stateId);
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

        foreach (var state in Game.States)
        {
            States[state.StateId] = state;
        }

        foreach (var edge in Game.Edges)
        {
            Edges[$"{edge.Source}.{edge.Type}.{edge.Edge}"] = edge.Destination;
        }
    }
}
