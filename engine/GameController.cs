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
        // create loader just for initializiation while we load modules.
        var initLoader = new YamlConfigLoader();
        initLoader.RegisterDeserializer(new ConditionYamlConverter());
        initLoader.RegisterDeserializer(new GameEdgeYamlConverter());
        initLoader.RegisterDeserializer(new GaneStateUpdateYamlConverter());
        initLoader.RegisterDeserializer(new Vector2YamlConverter());
        var config = new ConfigRepository(initLoader);

        // read top level game config
        GameConfig = config.Read<GameConfig>(GameConfigPath);

        // load all modules
        foreach (var modulePath in GameConfig.Modules)
        {
            var module = config.Read<Module>(modulePath);
            var moduleDirectory = string.Concat(modulePath.Reverse().SkipWhile(curr => curr != '/').Reverse());

            foreach (var (templateId, template) in module.Templates)
            {
                template.Scene = $"{moduleDirectory}/{template.Scene}";
                Templates[$"{module.Id}.{templateId}"] = template;
            }

            var pluginScene = GD.Load<PackedScene>($"{moduleDirectory}/{module.Plugins}");
            var pluginContainer = pluginScene.Instantiate() as Node2D;

            if (pluginContainer is null)
                throw new GameWizardInternalException($"Encountered invalid root node type while loading plugins for module at: {modulePath}");

            foreach (var child in pluginContainer.GetChildren().Cast<PluginController>())
                Plugins.Add(child);
        }

        // create actual config loader
        var loader = new YamlConfigLoader();
        loader.RegisterDeserializer(new ConditionYamlConverter());
        loader.RegisterDeserializer(new GameEdgeYamlConverter());
        loader.RegisterDeserializer(new GaneStateUpdateYamlConverter());
        loader.RegisterDeserializer(new Vector2YamlConverter());
        foreach (var plugin in Plugins)
            plugin.RegisterDeserializer(loader);
        Config = new ConfigRepository(loader);

        // load state definition into state repository
        State.Initialize(GameConfig.State);
    }

    private void LoadScene(string sceneId)
    {
        // validate scene can be loaded and get related config
        if (LoadedScenes.ContainsKey(sceneId))
            throw new InvalidSceneTransitionException($"Tried to load scene {sceneId} when scene already loaded");

        var scene = GameConfig.Scenes[sceneId];
        var templateId = scene.Template;
        var template = Templates[templateId];

        // instantiate scene
        var packedScene = GD.Load<PackedScene>(template.Scene);
        var godotScene = packedScene.Instantiate() as Node2D;
        if (godotScene is null)
            throw new GameWizardInternalException($"Invalid root node type for scene template {templateId}.");
        if (SceneFocusStack.Count > 0)
            godotScene.ZIndex = LoadedScenes[SceneFocusStack[0]].ZIndex + 1;

        // register event handler to let the template send outputs
        var controller = godotScene.AsTemplate(templateId);
        controller.OutputEmitted += HandleTemplateOutput;

        // update internal state
        LoadedScenes[sceneId] = godotScene;
        SceneFocusStack.Insert(0, sceneId);

        // add to game node tree and initialize scene
        AddChild(godotScene);
        controller.InitializeController(this, templateId, sceneId, scene.Config);
    }

    private void ProcessInputs()
    {
        // early return if there is no scene to process inputs for.  this should
        // only happen during initialization.
        if (SceneFocusStack.IsEmpty()) return;

        // check to see if the currently focused screen can handle the inputs
        var handled = ProcessInputForScene(SceneFocusStack[0]);

        // if not, traverse up focus stack until some always active scene supports it
        foreach (var sceneId in SceneFocusStack.Skip(1))
        {
            var scene = GameConfig.Scenes[sceneId];

            if (!handled && scene.AlwaysActive)
                handled = ProcessInputForScene(sceneId);
        }
    }

    private bool ProcessInputForScene(string sceneId)
    {
        var scene = GameConfig.Scenes[sceneId];
        var templateId = scene.Template;
        var template = Templates[templateId];
        var controller = LoadedScenes[sceneId].AsTemplate(templateId);

        // get all inputs for given scene
        var inputState = template.Inputs.ToDictionary(
            input => input,
            input => Input.IsActionJustPressed($"{templateId}.{input}"));

        // scene decides if inputs should continue
        return inputState.Any(kvp => kvp.Value) && controller.HandleInput(inputState);
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
            throw new GameWizardInternalException($"Tried to process a template output from an non-focused scene.");

        var sourceScene = GameConfig.Scenes[sourceSceneId];
        var sourceTemplateId = sourceScene.Template;
        var sourceTemplate = Templates[sourceTemplateId];

        // find applicable transition for output
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

        // if we didn't find one, try to create a smart default
        if (target is null)
        {
            var defaultType = sourceTemplate.Outputs[outputId].Default;
            GD.PushWarning($"Did not find a valid output edge for output {outputId}.{outputArg}, defaulting to {defaultType}.");
            target = new SceneTransition
            {
                Edge = new Edge
                {
                    Type = defaultType,
                    OutputId = outputId,
                    OutputArg = outputArg,
                },
            };
        }

        var output = sourceTemplate.Outputs[target.Edge.OutputId];

        // quit is always allowed and happens immediately
        if (target.Edge.Type == EdgeType.Quit)
        {
            GetTree().Quit();
            return;
        }

        // validate the type of edge is supported by the output
        if (!output.Allowed.Contains(target.Edge.Type))
            throw new InvalidGameStateException($"Output edge type {target.Edge.Type} not allowed for output {target.Edge.OutputId}");

        // if the edge type means we should unload the current scene, do so
        if (target.Edge.Type == EdgeType.ToSibling ||
            target.Edge.Type == EdgeType.ToParent)
        {
            UnloadCurrentScene();
        }

        // apply any state updates
        foreach (var update in target.Updates)
            State.Update(update);

        // load the new scene if the edge type has one
        if (target.Edge.Type == EdgeType.ToSibling ||
            target.Edge.Type == EdgeType.ToChild)
        {
            if (string.IsNullOrEmpty(target.Edge.Destination))
                throw new GameWizardInternalException($"Expected edge {sourceSceneId} => {outputId}.{outputArg} to have a destination.");
            LoadScene(target.Edge.Destination);
        }

        // if we're out of loaded scenes, quit
        if (SceneFocusStack.IsEmpty())
            throw new InvalidSceneTransitionException($"Zero scenes are loaded after transition {sourceSceneId} => {outputId}.{outputArg}.");

        // inform newly focused scene it is now in focus
        if (target.Edge.Type == EdgeType.ToChild ||
            target.Edge.Type == EdgeType.ToParent)
        {
            var newSceneId = SceneFocusStack[0];
            var newScene = GameConfig.Scenes[newSceneId];
            var newController = LoadedScenes[newSceneId].AsTemplate(newScene.Template);

            newController.HandleFocus(sourceSceneId, $"{outputId}.{outputArg}");
        }
    }
}
