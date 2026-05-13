using System.Collections.Generic;
using Godot;

namespace GameWizard.Engine;

public delegate void TemplateOutputEmittedHandler(string sourceScene, string outputId, string outputArg);

public abstract partial class TemplateController<T> : Node2D, ITemplateController where T : class
{
    public event TemplateOutputEmittedHandler OutputEmitted;

    public GameController Game { get; private set; }
    public string TemplateId { get; private set; }
    public string SceneId { get; private set; }

    protected T Config { get; private set; }

    public void InitializeController(GameController game, string templateId, string sceneId, string path)
    {
        Game = game;
        TemplateId = templateId;
        SceneId = sceneId;
        Config = Game.Config.Read<T>(path);
        InitializeScene();
    }

    protected void EmitOutput(string outputId, string outputArg)
    {
        OutputEmitted?.Invoke(SceneId, outputId, outputArg);
    }

    protected abstract void InitializeScene();
    public abstract bool HandleInput(IDictionary<string, bool> inputs);
    public abstract void HandleFocus(string sourceScene, string outputId);
}

public interface ITemplateController
{
    public event TemplateOutputEmittedHandler OutputEmitted;

    public GameController Game { get; }
    public string TemplateId { get; }
    public string SceneId { get; }

    public void InitializeController(GameController game, string templateId, string sceneId, string path);

    public bool HandleInput(IDictionary<string, bool> inputs);
    public void HandleFocus(string sourceScene, string outputId);
}