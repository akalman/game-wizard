using GameWizard.Engine;
using GameWizard.Engine.Util;
using Godot;

namespace GameWizard.Core.LandmarkOverworld;

public partial class LandmarkOverworldController : TemplateController<OverworldConfig>
{
    [Export] public TextureRect Background { get; set; }
    [Export] public Control LandmarkContainer { get; set; }

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
                Position = (landmark.Offset + Vector2.One) * new Vector2(960, 540),
                Visible = landmark.When.Evaluate(Game.State),
            };

            LandmarkContainer.AddChild(landmarkNode);
            landmarkNode.Pressed += () => EmitOutput("navigate", landmarkId);
        }
    }

    public override bool HandleInput(string input)
    {
        return false;
    }

    public override void HandleFocus(string sourceScene, string outputId)
    {

    }
}
