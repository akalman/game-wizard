using System;
using System.Collections.Generic;
using System.Linq;
using GameWizard.Engine.Config;
using GameWizard.Engine.Schema;
using GameWizard.Engine.State;
using GameWizard.Engine.Util;
using Godot;

namespace GameWizard.Engine;

public partial class GWGameController : Node2D
{
    [Export(PropertyHint.File, "*.yaml")] public string GameConfig { get; set; }

    public IConfigLoader ConfigLoader { get; set; } = new YamlConfigLoader();
    public GWStateFacade State { get; set; }

    public GWGame Game { get; set; }

    private IDictionary<string, GWTemplate> Templates { get; set; } = new Dictionary<string, GWTemplate>();
    private IDictionary<string, GWScreen> Screens { get; set; } = new Dictionary<string, GWScreen>();
    private IDictionary<string, IList<GWEdge>> Edges { get; set; } = new Dictionary<string, IList<GWEdge>>();

    private IDictionary<string, Node2D> LoadedScenes { get; } = new Dictionary<string, Node2D>();
    private IList<string> ScreenFocusStack { get; } = new List<string>();

    public override void _Ready()
    {
        InitializeGame();
        State.Create();
        LoadScreen(Game.InitialScreen);
    }

    public override void _Process(double delta)
    {
        ProcessInputs();
    }

    public void TransitionScreen(string edgeType, string edgeId)
    {
        // validate the transition
        var currentScreenId = ScreenFocusStack[0];
        var currentScreen = Screens[currentScreenId];
        var currentTemplate = Templates[currentScreen.Template];

        if (!currentTemplate.Events.ContainsKey(edgeType))
        {
            GD.PushError(
                $"Received invalid edge type {edgeType} for current screen template {currentTemplate.Id}.  Expected one of {currentTemplate.Events}.");
            GetTree().Quit();
            return;
        }

        // Edges[$"{currentScreenId}.{edgeType}.{edgeId}"]
        // find edge and fallback to wildcard id if needed
        IList<GWEdge> edges = [];
        if (Edges.ContainsKey($"{currentScreenId}.{edgeType}.{edgeId}"))
            edges = Edges[$"{currentScreenId}.{edgeType}.{edgeId}"];
        else if (Edges.ContainsKey($"{currentScreenId}.{edgeType}.ref://any"))
            edges = Edges[$"{currentScreenId}.{edgeType}.ref://any"];

        // find conditional destination
        var edge = edges.FirstOrDefault(edge => GWConditionEvaluator.Evaluate(State, edge.Conditions));
        if (edge == null)
        {
            GD.PushWarning($"Did not find valid conditional destination for {currentScreenId}.{edgeType}.{edgeId}.");
            // GetTree().Quit();
            return;
        }

        GD.PushWarning($"next screen {edge.Destination}");

        // apply effects
        foreach (var effect in edge.Changes)
        {
            switch (effect.Type)
            {
                case GWStateChangeType.SetFlag:
                    State.Write(effect.Target, effect.Value);
                    break;
            }
        }

        var targetScreenId = edge.Destination;

        // if the source screen is now terminal or the target screen is ref://parent, remove the current screen
        if (targetScreenId == "ref://parent" ||
            currentTemplate.Events[edge.Event] == GWEventLifecycle.Terminal)
        {
            LoadedScenes[ScreenFocusStack[0]].QueueFree();
            ScreenFocusStack.RemoveAt(0);
            if (ScreenFocusStack.Count > 0 && LoadedScenes[ScreenFocusStack[0]] is GWTemplateController controller)
                controller.ReceiveInput($"{currentScreen.Template}.{edgeType}.{edgeId}");
        }

        // quit if the destination is the close screen
        if (targetScreenId == Game.CloseScreen) GetTree().Quit();

        // load destination screen
        else if (targetScreenId != "ref://parent") LoadScreen(targetScreenId);

        // quit if we are left with no active screens
        if (ScreenFocusStack.Count == 0) GetTree().Quit();
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
                if (Input.IsActionJustPressed($"{screen.Template}.{input}"))
                {
                    if (controller.ReceiveInput($"{screen.Template}.{input}"))
                    {
                        return;
                    }
                }
            }

            // if this screen halts inputs don't process any further
            return;
        }
    }

    private void LoadScreen(string screenId)
    {
        GD.PushWarning($"loading screen {screenId}");
        var screen = Screens[screenId];
        var template = Templates[screen.Template];
        var packedScene = GD.Load<PackedScene>(template.Scene);

        var scene = packedScene.Instantiate() as Node2D;
        if (scene == null)
        {
            GD.PushError($"Loaded scene for template {template.Id} is not a Node2D scene.");
            GetTree().Quit();
            return;
        }

        var controller = scene as GWTemplateController;
        if (controller == null)
        {
            GD.PushError(
                $"Loaded scene for template {template.Id} has no GWTemplateController implementation at the root.");
            GetTree().Quit();
            return;
        }

        LoadedScenes[screenId] = scene;

        controller.ScreenId = screenId;
        controller.GameController = this;
        controller.State = State;
        scene.ZIndex = ScreenFocusStack.Count;

        ScreenFocusStack.Insert(0, screenId);

        AddChild(scene);

        controller.SetConfig(ConfigLoader, screen.Config);
    }

    private void InitializeGame()
    {
        Game = ConfigLoader.Load<GWGame>(GameConfig);
        State = new GWStateFacade(Game.State);

        foreach (var moduleId in Game.Modules)
        {
            var module = ConfigLoader.Load<GWModule>($"res://{moduleId}/config.yaml");
            foreach (var template in module.Templates)
            {
                Templates[$"{module.Id}.{template.Id}"] = template;
            }
        }

        foreach (var screen in Game.Screens)
        {
            Screens[screen.Id] = screen;
        }

        foreach (var edge in Game.Edges)
        {
            var edgeKey = $"{edge.Source}.{edge.Event}.{edge.EventId}";
            GD.PushWarning(edgeKey);
            if (!Edges.ContainsKey(edgeKey))
            {
                Edges[edgeKey] = new List<GWEdge>();
            }

            Edges[edgeKey].Add(edge);
        }
    }

    public void EmitEvent(GWTemplateController source, string eventId)
    {
        GD.PushWarning($"Processing Event {eventId}");

        if (source.ScreenId != ScreenFocusStack[0]) return;

        var screen = Screens[source.ScreenId];
        var template = Templates[screen.Template];

        if (!eventId.StartsWith(screen.Template)) throw new NotImplementedException();

        eventId = eventId.Substring(screen.Template.Length + 1);

        var eventIdParts = eventId.Split(".");

        if (!template.Events.ContainsKey(eventIdParts[0])) throw new NotImplementedException();

        TransitionScreen(eventIdParts[0], eventIdParts[1]);
    }
}
