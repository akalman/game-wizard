using System.Collections.Generic;
using Godot;

public partial class HardCodedLayerBasedSceneController : Node2D
{
	[Export]
	public Container SceneContainer { get; set; }

	[Export]
	public PackedScene SourceScene { get; set; }

	[Export]
	public PackedScene TargetScene { get; set; }

	public IDictionary<string, Node> LoadedScenes { get; set; } = new Dictionary<string, Node>();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var sourceNode = SourceScene.Instantiate();
		LoadedScenes["source"] = sourceNode;
        SceneContainer.AddChild(sourceNode);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("ui_accept"))
		{
			var targetNode = TargetScene.Instantiate();
			LoadedScenes["target"] = targetNode;
            SceneContainer.AddChild(targetNode);
		}
	}
}
