using GameWizard.Engine.Util;
using Godot;

namespace GameWizard.Engine;

public abstract partial class GWTemplateController<T> : Node2D, GWTemplateController where T : class
{
    public GWGameController GameController { get; set; }
    protected T Config { get; set; }

    public void SetConfig(IConfigLoader loader, string path)
    {
        Config = loader.Load<T>(path);
        ConfigUpdated();
    }

    public abstract bool ReceiveInput(string input);
    protected abstract void ConfigUpdated();
}

public interface GWTemplateController
{
    public GWGameController GameController { get; set; }
    public void SetConfig(IConfigLoader loader, string path);
    public bool ReceiveInput(string command);
}
