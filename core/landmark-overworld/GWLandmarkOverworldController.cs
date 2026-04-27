using System.Collections.Generic;
using GameWizard.Engine;
using GameWizard.Engine.Util;
using Godot;

namespace GameWizard.Core;

public partial class GWLandmarkOverworldController : GWTemplateController<GWLandmarkOverworld>
{
    [Export] public Control BackgroundContainer { get; set; }
    [Export] public Control LandmarkContainer { get; set; }

    private IDictionary<string, GWLandmarkOverworldLandmark> Landmarks { get; set; } = new Dictionary<string, GWLandmarkOverworldLandmark>();
    private IDictionary<string, TextureButton> Buttons { get; set; } = new Dictionary<string, TextureButton>();

    public override void _ExitTree()
    {
        if (State is not null) State.StateUpdated -= RefreshVisibility;
    }

    protected override void ConfigUpdated()
    {
        foreach (var background in BackgroundContainer.GetChildren())
        {
            background.QueueFree();
        }

        foreach (var sprite in LandmarkContainer.GetChildren())
        {
            sprite.QueueFree();
        }

        var newBackground = new TextureRect
        {
            Texture = GD.Load<Texture2D>(Config.Map.TexturePath),
            StretchMode = TextureRect.StretchModeEnum.Scale,
            ExpandMode = Config.Map.ScalingType.ToExpandMode(),
            CustomMinimumSize = new Vector2(1920, 1080),
        };
        BackgroundContainer.AddChild(newBackground);

        foreach (var landmark in Config.Landmarks)
        {
            Landmarks[landmark.LandmarkId] = landmark;

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
            Buttons[landmark.Edge] = button;
            button.Pressed += () => HandleButtonPressed(landmark.Edge);

            button.Visible = GWConditionEvaluator.Evaluate(State, landmark.Conditions);
        }

        State.StateUpdated += RefreshVisibility;
    }

    public override bool ReceiveInput(string command)
    {
        return false;
    }

    private void HandleButtonPressed(string destination)
    {
        GameController.TransitionScreen("navigate", destination);
    }

    private void RefreshVisibility()
    {
        foreach (var landmarkId in Landmarks.Keys)
        {
            var landmark = Landmarks[landmarkId];
            var button = Buttons[landmark.Edge];
            button.Visible = GWConditionEvaluator.Evaluate(State, landmark.Conditions);
        }
    }
}
