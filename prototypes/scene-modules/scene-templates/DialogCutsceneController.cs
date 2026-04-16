using Godot;
using Prototypes.SceneModules.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Prototypes.SceneModules.SceneTemplates;

public partial class DialogCutsceneController : SceneTemplateController<GWDialog, GWDialogCommands>
{
    [Export] public RichTextLabel MenuBar { get; set; }
    [Export] public HBoxContainer LeftCharactersContainer { get; set; }
    [Export] public HBoxContainer RightCharactersContainer { get; set; }
    [Export] public RichTextLabel DialogBox { get; set; }

    private IDictionary<string, TextureRect> CurrentCharacters { get; set; } = new Dictionary<string, TextureRect>();
    private GWDialogFrame CurrentFrame { get; set; }

    private IDictionary<string, GWDialogFrame> Frames { get; set; } = new Dictionary<string, GWDialogFrame>();
    private IDictionary<string, GWDialogCharacter> Characters { get; set; } = new Dictionary<string, GWDialogCharacter>();
    
    public override void ConfigUpdated()
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

    public override void ReceiveCommand(GWDialogCommands command)
    {
        foreach (var edge in CurrentFrame.Edges)
        {
            if (edge.Command == command)
            {
                if (edge.DestinationFrame == "_exit")
                {
                    QueueFree();
                    return;
                }

                TransitionToFrame(edge.DestinationFrame);
                return;
            }
        }
        GD.PushWarning("did not find appropriate edge for command.");
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
                CurrentCharacters[characterId].QueueFree();
                CurrentCharacters.Remove(characterId);
            }

            foreach (var characterId in destinationFrame.State.LeftCharacterIds)
            {
                var character = Characters[characterId];
                var sprite = new TextureRect
                {
                    Texture = GD.Load<Texture2D>(character.SpritePath),
                    ExpandMode = TextureRect.ExpandModeEnum.FitWidth,
                };
                CurrentCharacters[characterId] = sprite;
                LeftCharactersContainer.AddChild(sprite);
            }
        }

        // update right characters
        var targetRightCharacterIds = destinationFrame.State.RightCharacterIds ?? [];
        if (!targetRightCharacterIds.SequenceEqual(CurrentFrame.State.RightCharacterIds))
        {
            foreach (var characterId in CurrentFrame.State.RightCharacterIds)
            {
                CurrentCharacters[characterId].QueueFree();
                CurrentCharacters.Remove(characterId);
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
                CurrentCharacters[characterId] = sprite;
                RightCharactersContainer.AddChild(sprite);
                zindex -= 1;
            }
        }

        // TODO: add menu bar logic

        CurrentFrame = destinationFrame;
    }
}
