using System.Collections.Generic;
using Godot;
using Prototypes.SceneModules.DataModel;
using Prototypes.SceneModules.SceneTemplates;

public partial class LandmarkOverworldController :   SceneTemplateController<GWOverworld>
{
	[Export]
	public Control BackgroundContainer { get; set; }

	[Export]
	public Control LandmarkContainer { get; set; }

	public IDictionary<string, TextureButton> Buttons { get; set; }

	public bool IsActive { get; set; }

	protected override void ConfigUpdated()
	{
		GD.PushWarning("in overworld config update");
		foreach (var background in BackgroundContainer.GetChildren())
		{
			background.QueueFree();
		}

		foreach (var sprite in LandmarkContainer.GetChildren())
		{
			sprite.QueueFree();
		}

		Buttons = new Dictionary<string, TextureButton>();

		var newBackground = new TextureRect
		{
			Texture = GD.Load<Texture2D>(Config.Map.MapTexturePath),
			StretchMode = TextureRect.StretchModeEnum.Scale,
			ExpandMode = Config.Map.ScalingType.ToExpandMode(),
			CustomMinimumSize = new Vector2(1920, 1080),
		};
		BackgroundContainer.AddChild(newBackground);

		foreach (var landmark in Config.Landmarks)
		{
			var container = new CenterContainer
			{
				UseTopLeft = true,
				Position = landmark.Offset * new Vector2(960, 540),
			};

			var texture = GD.Load<Texture2D>(landmark.TexturePath);
			var button = new TextureButton
			{
				TextureNormal = texture,
				IgnoreTextureSize = true,
				StretchMode = TextureButton.StretchModeEnum.KeepAspectCentered,
				CustomMinimumSize = landmark.Size,
				FocusMode = Control.FocusModeEnum.None
			};

			container.AddChild(button);
			LandmarkContainer.AddChild(container);
			Buttons[landmark.Destination] = button;
			button.Pressed += () => HandleButtonPressed(landmark.Destination);
		}

		IsActive = true;
	}

	public override bool ReceiveCommand(string command)
	{
		GD.PushWarning("does not currently support commands.");
		return false;
	}

	public override void _Process(double delta)
	{
		// if (IsActive)
		// {
		// 	foreach (var (destination, button) in Buttons)
		// 	{
		// 		if (button.ButtonPressed)
		// 		{
		// 			IsActive = false;
		// 			GameController.TransitionScene(destination);
		// 			Reactivate();
		// 		}
		// 	}
		// }
	}

	private void HandleButtonPressed(string destination)
	{
		GameController.TransitionScene(destination);
	}

	private async void Reactivate()
	{
		await ToSignal(GetTree().CreateTimer(1.0), SceneTreeTimer.SignalName.Timeout);
		IsActive = true;
	}
}
