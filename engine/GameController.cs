using System;
using System.Collections.Generic;
using System.Linq;
using GameWizard.Engine.Config;
using GameWizard.Engine.Config.Yaml;
using GameWizard.Engine.Database;
using GameWizard.Engine.Schema.Game;
using GameWizard.Engine.Schema.Modules;
using GameWizard.Engine.State;
using GameWizard.Engine.Util;
using Godot;

namespace GameWizard.Engine;

public partial class GameController : Node2D
{
    [Export(PropertyHint.File, "*.yaml")] public string GameConfigPath { get; set; }

    public GameConfig GameConfig { get; set; }

    public IConfigRepository Config { get; set; }
    public IStateRepository State { get; set; } = new StateRepository();
    public IDatabaseRepository Db { get; set; } = new DatabaseRepository();

    private IList<PluginController> Plugins { get; set; } = new List<PluginController>();
    private IDictionary<string, Template> Templates { get; set; } = new Dictionary<string, Template>();
    private IDictionary<string, GameScene> Scenes { get; set; } = new Dictionary<string, GameScene>();

    private IDictionary<string, Node2D> LoadedScenes { get; set; } = new Dictionary<string, Node2D>();
    private IList<string> SceneFocusStack { get; set; } = new List<string>();

    public override void _Ready()
    {
        InitializeGame();
        State.Create();
        LoadScene(GameConfig.InitialScene);
    }

    public override void _Process(double delta)
    {
        ProcessInputs();
    }

    private void InitializeGame()
    {
        Plugins = GetChildren().Cast<PluginController>().ToList();

        var loader = new YamlConfigLoader();

        loader.RegisterDeserializer(new ConditionYamlConverter());
        loader.RegisterDeserializer(new GameEdgeYamlConverter());
        loader.RegisterDeserializer(new GaneStateUpdateYamlConverter());
        loader.RegisterDeserializer(new Vector2YamlConverter());

        foreach (var plugin in Plugins)
            plugin.RegisterDeserializer(loader);

        Config = new ConfigRepository(loader);

        GameConfig = Config.Read<GameConfig>(GameConfigPath);

        foreach (var modulePath in GameConfig.Modules)
        {
            var module = Config.Read<Module>(modulePath);

            foreach (var (templateId, template) in module.Templates)
                Templates[$"{module.Id}.{templateId}"] = template;
        }

        Scenes = GameConfig.Scenes;

        State.Initialize(GameConfig.State);
    }

    private void LoadScene(string sceneId)
    {
        if (LoadedScenes.ContainsKey(sceneId))
            throw new InvalidSceneTransitionException($"Tried to load scene {sceneId} when scene already loaded");

        var scene = Scenes[sceneId];
        var templateId = scene.Template;
        var template = Templates[templateId];

        var packedScene = GD.Load<PackedScene>(template.Scene);
        var godotScene = packedScene.Instantiate() as Node2D;
        if (godotScene is null)
            throw new GameWizardInternalException($"Invalid root node type for scene template {templateId}.");
        if (SceneFocusStack.Count > 0)
            godotScene.ZIndex = LoadedScenes[SceneFocusStack[0]].ZIndex + 1;

        var controller = godotScene.AsTemplate(templateId);
        controller.OutputEmitted += HandleTemplateOutput;

        LoadedScenes[sceneId] = godotScene;
        SceneFocusStack.Insert(0, sceneId);

        AddChild(godotScene);
        controller.InitializeController(this, templateId, sceneId, scene.Config);
    }

    private void ProcessInputs()
    {
        if (SceneFocusStack.IsEmpty()) return;

        var handled = ProcessInputForScene(SceneFocusStack[0]);

        foreach (var sceneId in SceneFocusStack.Skip(1))
        {
            var scene = Scenes[sceneId];

            if (!handled && scene.AlwaysActive)
                handled = ProcessInputForScene(sceneId);
        }
    }

    private bool ProcessInputForScene(string sceneId)
    {
        var scene = Scenes[sceneId];
        var templateId = scene.Template;
        var template = Templates[templateId];
        var controller = LoadedScenes[sceneId].AsTemplate(templateId);

        foreach (var input in template.Inputs)
        {
            if (Input.IsActionJustPressed($"{templateId}.{input}"))
                if (controller.HandleInput(input))
                    return true;
        }

        return false;
    }

    private void UnloadCurrentScene()
    {
        if (SceneFocusStack.IsEmpty())
            throw new GameWizardInternalException($"Tried to unload current scene when there is no current scene.");

        LoadedScenes[SceneFocusStack[0]].QueueFree();
        LoadedScenes.Remove(SceneFocusStack[0]);
        SceneFocusStack.RemoveAt(0);
    }

    private void HandleTemplateOutput(string sourceSceneId, string outputId, string outputArg)
    {
        // TODO: add support for processing outputs from scenes not in focus
        if (sourceSceneId != SceneFocusStack[0])
            throw new NotImplementedException();

        var sourceScene = Scenes[sourceSceneId];
        var sourceTemplateId = sourceScene.Template;
        var sourceTemplate = Templates[sourceTemplateId];

        SceneTransition target = null;

        foreach (var transition in sourceScene.Transitions)
        {
            if (transition.Edge.OutputId == outputId &&
                transition.Edge.OutputArg == outputArg &&
                transition.When.Evaluate(State))
            {
                target = transition;
            }
        }

        if (target is null)
            throw new InvalidGameStateException($"Did not find a valid output edge for output {outputId}.{outputArg}.");

        var output = sourceTemplate.Outputs[target.Edge.OutputId];

        if (!output.Allowed.Contains(target.Edge.Type))
            throw new InvalidGameStateException(
                $"Output edge type {target.Edge.Type} not allowed for output {target.Edge.OutputId}");

        if (target.Edge.Type == EdgeType.Quit)
        {
            GetTree().Quit();
            return;
        }

        if (target.Edge.Type == EdgeType.ToSibling ||
            target.Edge.Type == EdgeType.ToParent)
        {
            UnloadCurrentScene();
        }

        foreach (var update in target.Updates)
            State.Update(update);

        if (target.Edge.Type == EdgeType.ToSibling ||
            target.Edge.Type == EdgeType.ToChild)
        {
            if (string.IsNullOrEmpty(target.Edge.Destination))
                throw new GameWizardInternalException(
                    $"Expected edge {sourceSceneId} => {outputId}.{outputArg} to have a destination.");
            LoadScene(target.Edge.Destination);
        }

        if (SceneFocusStack.IsEmpty())
            throw new InvalidSceneTransitionException(
                $"Zero scenes are loaded after transition {sourceSceneId} => {outputId}.{outputArg}.");

        if (target.Edge.Type == EdgeType.ToChild ||
            target.Edge.Type == EdgeType.ToParent)
        {
            var newSceneId = SceneFocusStack[0];
            var newScene = Scenes[newSceneId];
            var newController = LoadedScenes[newSceneId].AsTemplate(newScene.Template);

            newController.HandleFocus(sourceSceneId, $"{outputId}.{outputArg}");
        }
    }
}
