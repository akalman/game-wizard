using System.Collections.Generic;
using System.Linq;
using Godot;
using Prototypes.SceneModules.DataModel;

namespace Prototypes.SceneModules.SceneTemplates;

public partial class DialogCutsceneController : SceneTemplateController<GWDialog>
{
    [Export] public RichTextLabel MenuBar { get; set; }
    [Export] public HBoxContainer LeftCharactersContainer { get; set; }
    [Export] public HBoxContainer RightCharactersContainer { get; set; }
    [Export] public RichTextLabel DialogBox { get; set; }

    private IDictionary<string, TextureRect> CurrentCharacters { get; set; } = new Dictionary<string, TextureRect>();
    private GWDialogFrame CurrentFrame { get; set; }

    private IDictionary<string, GWDialogFrame> Frames { get; set; } = new Dictionary<string, GWDialogFrame>();
    private IDictionary<string, GWDialogCharacter> Characters { get; set; } = new Dictionary<string, GWDialogCharacter>();

    protected override void ConfigUpdated()
    {
        GD.PushWarning("in dialog config update");
        foreach (var (_, character) in CurrentCharacters)
        {
            character.QueueFree();
        }
        MenuBar.Text = "Menu Bar";
        DialogBox.Text = string.Empty;

        CurrentCharacters = new Dictionary<string, TextureRect>();

        Frames = Config.Frames.ToDictionary(frame => frame.FrameId);
        Characters = Config.Characters.ToDictionary(character => character.CharacterId);

        CurrentFrame = new GWDialogFrame
        {
            State = new GWDialogFrameComponentState
            {
                DialogText = string.Empty,
                LeftCharacterIds = [],
                RightCharacterIds = [],
            },
            Edges = [],
        };
        TransitionToFrame(Config.InitialFrameId);
    }

    public override bool ReceiveCommand(string command)
    {
        foreach (var edge in CurrentFrame.Edges)
        {
            if (edge.Command == command)
            {
                if (edge.DestinationFrame == "_exit")
                {
                    GameController.TransitionScene(Config.TerminalEdge);
                    return true;
                }

                TransitionToFrame(edge.DestinationFrame);
                return true;
            }
        }
        GD.PushWarning("did not find appropriate edge for command.");
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
