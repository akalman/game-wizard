using System.Collections.Generic;
using System.Linq;
using GameWizard.Engine.Config;
using GameWizard.Engine.Config.Yaml;
using GameWizard.Engine.Database;
using GameWizard.Engine.Schema.Game;
using GameWizard.Engine.Schema.Logic;
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
    public IDatabaseRepository Db { get; set; }

    private IList<PluginController> Plugins { get; set; } = new List<PluginController>();
    private IDictionary<string, Template> Templates { get; set; } = new Dictionary<string, Template>();

    private IDictionary<string, Node2D> LoadedScenes { get; set; } = new Dictionary<string, Node2D>();
    private IList<string> SceneFocusStack { get; set; } = new List<string>();
    private ITemplateController CurrentController => LoadedScenes[SceneFocusStack[0]].AsTemplate(GameConfig.Scenes[SceneFocusStack[0]].Template);

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
        initLoader.RegisterDeserializer(GrammarParserYamlConverter<ICondition>.Create(ConditionParser.Parsers));
        initLoader.RegisterDeserializer(GrammarParserYamlConverter<ISceneEdge>.Create(SceneEdgeParser.Mappers));
        initLoader.RegisterDeserializer(GrammarParserYamlConverter<IStateUpdate>.Create(StateUpdateParser.Mappers));
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
        loader.RegisterDeserializer(GrammarParserYamlConverter<ICondition>.Create(ConditionParser.Parsers));
        initLoader.RegisterDeserializer(GrammarParserYamlConverter<ISceneEdge>.Create(SceneEdgeParser.Mappers));
        loader.RegisterDeserializer(GrammarParserYamlConverter<IStateUpdate>.Create(StateUpdateParser.Mappers));
        loader.RegisterDeserializer(new Vector2YamlConverter());
        loader.RegisterDeserializer(new DbEntryYamlConverter());
        foreach (var plugin in Plugins)
            plugin.RegisterDeserializer(loader);
        Config = new ConfigRepository(loader);

        // load state definition into state repository
        State.Initialize(GameConfig.State);

        // load database entries into database
        Db = new DatabaseRepository(Config);
        foreach (var (databaseId, dbConfigPath) in GameConfig.Database)
            Db.RegisterDb(databaseId, Config.Read<GameDb>(dbConfigPath));
    }

    private ITemplateController LoadScene(string sceneId)
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
        return controller;
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

    private void HandleTemplateOutput(string sourceSceneId, string outputType, string outputArg)
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
            if (transition.Edge.OutputId == outputType &&
                transition.Edge.OutputArg == outputArg &&
                transition.When.Evaluate(State))
            {
                target = transition;
                break;
            }
        }

        // if we didn't find one, try to create a smart default
        if (target is null)
        {
            var defaultType = sourceTemplate.Outputs[outputType].Default;
            GD.PushWarning($"Did not find a valid output edge for output {outputType}.{outputArg}, defaulting to {defaultType}.");
            ISceneEdge defaultEdge = defaultType switch
            {
                EdgeType.ToParent => new ToParentEdge { OutputId = outputType, OutputArg = outputArg },
                EdgeType.ToSelf => new ToSelfEdge { OutputId = outputType, OutputArg = outputArg },
                _ => throw new GameWizardInternalException(),
            };
            target = new SceneTransition { Edge = defaultEdge };
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
        if (target.Edge.Type is EdgeType.ToSibling or EdgeType.ToParent)
        {
            UnloadCurrentScene();
        }

        // apply any state updates
        foreach (var update in target.Updates)
            State.Update(update);

        // inform current scene if its losing focus or not
        if (target.Edge.Type is EdgeType.ToChild)
            LoadedScenes[sourceSceneId].AsTemplate(sourceScene.Template).HandleFocusUpdated(sourceSceneId, $"{outputType}.{outputArg}", FocusState.Lost);
        if (target.Edge.Type is EdgeType.ToSelf)
            LoadedScenes[sourceSceneId].AsTemplate(sourceScene.Template).HandleFocusUpdated(sourceSceneId, $"{outputType}.{outputArg}", FocusState.Retained);

        // load the new scene if the edge type has one
        if (target.Edge is ToSiblingEdge || target.Edge is ToChildEdge)
        {
            string destination = null;
            if (target.Edge is ToSiblingEdge toSibling) destination = toSibling.Destination;
            if (target.Edge is ToChildEdge toChild) destination = toChild.Destination;
            if (string.IsNullOrEmpty(destination))
                throw new GameWizardInternalException($"Expected edge {sourceSceneId} => {outputType}.{outputArg} to have a destination.");

            var newController = LoadScene(destination);
            newController.HandleFocusUpdated(sourceSceneId, $"{outputType}.{outputArg}", FocusState.Gained);
        }

        // inform old scene it has regained focus
        if (target.Edge is ToParentEdge)
            CurrentController.HandleFocusUpdated(sourceSceneId, $"{outputType}.{outputArg}", FocusState.Gained);

        // if we're out of loaded scenes, quit
        if (SceneFocusStack.IsEmpty())
            throw new InvalidSceneTransitionException($"Zero scenes are loaded after transition {sourceSceneId} => {outputType}.{outputArg}.");
    }
}
