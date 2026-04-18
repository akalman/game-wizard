using Godot;
using System;

namespace Prototypes.SceneModules.SceneTemplates;

public abstract partial class SceneTemplateController<T> : Node2D, SceneTemplateController where T : class
{
	[Export]
	protected T Config { get; set; }

    [Export]
    public SceneModulesGameController GameController { get; set; }
    
    public void SetConfig(Object config)
    {
	    Config = config as T;
        ConfigUpdated();
    }

	protected abstract void ConfigUpdated();
	public abstract bool ReceiveCommand(string command);

}

public interface SceneTemplateController
{
	public SceneModulesGameController GameController { get; set; }
	public void SetConfig(Object config);
	public bool ReceiveCommand(string command);
}
