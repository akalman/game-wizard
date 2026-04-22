using System.Collections.Generic;
using System.Linq;
using GameWizard.Engine;
using Godot;
using Character = GameWizard.Core.GWDialogCutsceneCharacter;
using Frame = GameWizard.Core.GWDialogCutsceneFrame;
using State = GameWizard.Core.GWDialogCutsceneFrameState;

namespace GameWizard.Core;

public partial class GWDialogCutsceneController : GWTemplateController<GWDialogCutscene>
{
    [Export] public RichTextLabel MenuBar { get; set; }
    [Export] public HBoxContainer LeftCharactersContainer { get; set; }
    [Export] public HBoxContainer RightCharactersContainer { get; set; }
    [Export] public RichTextLabel DialogBox { get; set; }

    private IDictionary<string, Frame> Frames { get; set; } = new Dictionary<string, Frame>();
    private IDictionary<string, Character> Characters { get; set; } = new Dictionary<string, Character>();

    private IDictionary<string, TextureRect> CurrentCharacters { get; set; } = new Dictionary<string, TextureRect>();
    private Frame CurrentFrame { get; set; }

    protected override void ConfigUpdated()
    {
        foreach (var (_, character) in CurrentCharacters)
        {
            character.QueueFree();
        }

        MenuBar.Text = "Menu Bar";
        DialogBox.Text = string.Empty;

        CurrentCharacters = new Dictionary<string, TextureRect>();

        Frames = Config.Frames.ToDictionary(frame => frame.FrameId);
        Characters = Config.Characters.ToDictionary(character => character.CharacterId);

        CurrentFrame = new Frame
        {
            State = new State
            {
                DialogText = string.Empty,
                LeftCharacterIds = [],
                RightCharacterIds = [],
            },
            Edges = [],
        };
        TransitionToFrame(Config.InitialFrameId);
    }

    public override bool ReceiveInput(string input)
    {
        foreach (var edge in CurrentFrame.Edges)
        {
            if (edge.Input == input)
            {
                if (string.IsNullOrEmpty(edge.DestinationFrame))
                {
                    GameController.TransitionScene("complete", Config.CompleteEdge);
                    return true;
                }

                TransitionToFrame(edge.DestinationFrame);
                return true;
            }
        }

        return false;
    }

    private void TransitionToFrame(string destinationFrameId)
    {
        var destinationFrame = Frames[destinationFrameId];

        // update dialog box
        if (destinationFrame.State.DialogText != CurrentFrame.State.DialogText)
        {
            DialogBox.Text = destinationFrame.State.DialogText;
        }

        // update left characters
        if (!destinationFrame.State.LeftCharacterIds.SequenceEqual(CurrentFrame.State.LeftCharacterIds))
        {
            foreach (var characterId in CurrentFrame.State.LeftCharacterIds)
            {
                CurrentCharacters[$"left.${characterId}"].QueueFree();
                CurrentCharacters.Remove($"left.${characterId}");
            }

            foreach (var characterId in destinationFrame.State.LeftCharacterIds)
            {
                var character = Characters[characterId];
                var sprite = new TextureRect
                {
                    Texture = GD.Load<Texture2D>(character.SpritePath),
                    ExpandMode = TextureRect.ExpandModeEnum.FitWidth,
                };
                CurrentCharacters[$"left.${characterId}"] = sprite;
                LeftCharactersContainer.AddChild(sprite);
            }
        }

        // update right characters
        var targetRightCharacterIds = destinationFrame.State.RightCharacterIds ?? [];
        if (!targetRightCharacterIds.SequenceEqual(CurrentFrame.State.RightCharacterIds))
        {
            foreach (var characterId in CurrentFrame.State.RightCharacterIds)
            {
                CurrentCharacters[$"right.${characterId}"].QueueFree();
                CurrentCharacters.Remove($"right.${characterId}");
            }

            var zindex = 0;
            foreach (var characterId in targetRightCharacterIds.Reverse())
            {
                var character = Characters[characterId];
                var sprite = new TextureRect
                {
                    Texture = GD.Load<Texture2D>(character.SpritePath),
                    ExpandMode = TextureRect.ExpandModeEnum.FitWidth,
                    FlipH = true,
                    ZIndex = zindex
                };
                CurrentCharacters[$"right.${characterId}"] = sprite;
                RightCharactersContainer.AddChild(sprite);
                zindex -= 1;
            }
        }

        // TODO: add menu bar logic

        CurrentFrame = destinationFrame;
    }
}
