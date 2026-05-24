using System.Collections.Generic;
using GameWizard.Engine;
using GameWizard.Engine.Util;
using Godot;

namespace GameWizard.Core.LandmarkOverworld;

public partial class LandmarkOverworldController : TemplateController<OverworldConfig>
{
    [Export] public TextureRect Background { get; set; }
    [Export] public Control LandmarkContainer { get; set; }

    private IDictionary<string, TextureButton> LoadedLandmarks { get; set; } = new Dictionary<string, TextureButton>();

    protected override void InitializeScene()
    {
        Background.Texture = GD.Load<Texture2D>(Config.Map.Sprite);
        Background.ExpandMode = Config.Map.Scaling switch
        {
            MapScaling.ActualSize => TextureRect.ExpandModeEnum.KeepSize,
            MapScaling.FitWidth => TextureRect.ExpandModeEnum.FitHeightProportional,
            MapScaling.FitHeight => TextureRect.ExpandModeEnum.FitWidthProportional,
            _ => throw new GameWizardInternalException(),
        };

        foreach (var (landmarkId, landmark) in Config.Landmarks)
        {
            var landmarkNode = new TextureButton
            {
                TextureNormal = GD.Load<Texture2D>(landmark.Sprite),
                CustomMinimumSize = landmark.Size,
                StretchMode = TextureButton.StretchModeEnum.KeepAspectCentered,
                IgnoreTextureSize = true,
                Position = (landmark.Offset + Vector2.One) * new Vector2(960, 540),
                Visible = landmark.When.Evaluate(Game.State),
                FocusMode = Control.FocusModeEnum.None,
            };

            LandmarkContainer.AddChild(landmarkNode);
            landmarkNode.Pressed += () => EmitOutput("navigate", landmarkId);
            LoadedLandmarks[landmarkId] = landmarkNode;
        }
    }

    public override bool HandleInput(IDictionary<string, bool> inputs)
    {
        // TODO: add input support

        return false;
    }

    public override void HandleFocusUpdated(string sourceScene, string outputId, FocusState updatedState)
    {
        GD.PushWarning("recalcuing landmark visibility");
        foreach (var (landmarkId, landmarkButton) in LoadedLandmarks)
            landmarkButton.Visible = Config.Landmarks[landmarkId].When.Evaluate(Game.State);
    }
}
