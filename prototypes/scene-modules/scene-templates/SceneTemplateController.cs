using Godot;
using Prototypes.SceneModules.DataModel;
using System;
using System.Collections.Generic;

namespace Prototypes.SceneModules.SceneTemplates;

public abstract partial class SceneTemplateController<T, U> : Node2D
{
	private T _config;

	[Export]
	public T Config
	{
		get => _config;
		set { _config = value; if (IsReady) ConfigUpdated(); }
	}

    [Export]
    public SceneModulesGameController GameController { get; set; }
    
    private bool IsReady { get; set; }

	public override void _Ready()
	{
        IsReady = true;
        ConfigUpdated();
	}

	public abstract void ConfigUpdated();

	public abstract void ReceiveCommand(U command);

}
