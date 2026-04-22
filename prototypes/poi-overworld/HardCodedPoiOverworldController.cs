using System.Collections.Generic;
using Godot;
using Godot.Collections;

public partial class HardCodedPoiOverworldController : Node2D
{
	[Export]
	public Sprite2D BackgroundContainer { get; set; }

	[Export]
	public Node2D PoiContainer { get; set; }

	private IList<OverworldPoi> POIs { get; set; }
    private IList<TextureButton> PoiObjects { get; set; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		POIs =
		[
			new OverworldPoi {
				Location = Vector2.Left * 0.2f + Vector2.Up * 0.2f,
				SpritePath = "sprites/10847340.png"
			},
			new OverworldPoi {
				Location = Vector2.Right * 0.2f + Vector2.Down * 0.2f,
				SpritePath = "sprites/10935928.png"
			},
		];
		PoiObjects = new Array<TextureButton>();

		var buttonScale = 0.1f;
		foreach (var poi in POIs)
		{
			var texture = GD.Load<Texture2D>($"res://prototypes/poi-overworld/{poi.SpritePath}");
			var textureSize = texture.GetSize();
			var button =  new TextureButton()
			{
				TextureNormal = texture,
                TextureHover = texture,
                TexturePressed = texture,
				Position = poi.Location * 500,
				Scale = Vector2.One * buttonScale,
                AnchorLeft = 0.5f,
                AnchorRight = 0.5f,
                AnchorTop = 0.5f,
                AnchorBottom = 0.5f,
                OffsetLeft = -textureSize.X * 0.5f * buttonScale + 960 * poi.Location.X,
                OffsetRight = textureSize.X * 0.5f * buttonScale - 960 * poi.Location.X,
                OffsetTop = -textureSize.Y * 0.5f * buttonScale + 540 * poi.Location.Y,
                OffsetBottom = textureSize.Y * 0.5f * buttonScale - 540 * poi.Location.Y,
			};
			PoiContainer.AddChild(button);
			PoiObjects.Add(button);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		foreach (var button in PoiObjects)
		{
			if (button.ButtonPressed)
			{
				GD.PushWarning($"Registered click on poi {button.TextureNormal.ResourcePath}");
			}
		}
	}

	private class OverworldPoi
	{
		public Vector2 Location { get; set; }
		public string SpritePath { get; set; }
	}
}
