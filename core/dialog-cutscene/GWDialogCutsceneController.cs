using System;
using System.Collections.Generic;
using System.Linq;
using GameWizard.Engine;
using GameWizard.Engine.State;
using Godot;

namespace GameWizard.Core;

public partial class GWDialogCutsceneController : GWTemplateController<GWDialogCutscene>
{
    [Export] public RichTextLabel MenuBar { get; set; }
    [Export] public HBoxContainer LeftCharactersContainer { get; set; }
    [Export] public HBoxContainer RightCharactersContainer { get; set; }
    [Export] public RichTextLabel DialogBox { get; set; }

    private IDictionary<string, GWDialogBlock> Blocks { get; set; } = new Dictionary<string, GWDialogBlock>();

    private IDictionary<string, GWDialogCharacter> Characters { get; set; } =
        new Dictionary<string, GWDialogCharacter>();

    private IDictionary<string, TextureRect> CurrentCharacters { get; set; } = new Dictionary<string, TextureRect>();
    private GWDialogBlock CurrentBlock { get; set; }
    private Queue<GWDialogFrameUpdate> Updates { get; set; }

    protected override void ConfigUpdated()
    {
        ClearDialog();

        Blocks = Config.Blocks.ToDictionary(block => block.Id);
        Characters = Config.Characters.ToDictionary(character => character.Id);

        LoadBlock(Config.InitialBlock);
    }

    public override bool ReceiveInput(string input)
    {
        GD.PushWarning($"Received input: {input}");

        switch (input)
        {
            case "core.dialog-cutscene.advance":
                ProcessAdvance();
                return true;
            case "core.dialog-cutscene.skip":
                throw new NotImplementedException();
            default:
                if (Updates.Count > 0) return false;

                var restartEdge = CurrentBlock.Edges.FirstOrDefault(edge =>
                    edge.EventId == input && GWConditionEvaluator.Evaluate(State, edge.Conditions));

                if (restartEdge is null) throw new NotImplementedException();

                LoadBlock(restartEdge.Destination);
                return true;
        }
    }

    private void ClearDialog()
    {
        foreach (var (_, character) in CurrentCharacters)
        {
            character.QueueFree();
        }

        MenuBar.Text = string.Empty;
        DialogBox.Text = string.Empty;

        CurrentCharacters = new Dictionary<string, TextureRect>();
        CurrentBlock = null;
        Updates = null;
    }

    private void LoadBlock(string blockId)
    {
        var block = Blocks[blockId];

        CurrentBlock = block;
        Updates = new Queue<GWDialogFrameUpdate>(block.Updates);

        GameController.EmitEvent(this, $"core.dialog-cutscene.begin-block.{blockId}");

        ProcessAdvance();
    }

    private void ProcessAdvance()
    {
        if (Updates.Count == 0)
        {
            var endBlockEvent = $"core.dialog-cutscene.end-block.{CurrentBlock.Id}";
            GameController.EmitEvent(this, endBlockEvent);

            GD.PushWarning($"processing block switch.  currently in {CurrentBlock.Id} looking for {endBlockEvent}");

            var targetEdge = CurrentBlock.Edges.FirstOrDefault(edge =>
                edge.EventId == endBlockEvent && GWConditionEvaluator.Evaluate(State, edge.Conditions));

            if (targetEdge is null) throw new NotImplementedException();

            var targetBlock = targetEdge.Destination;

            if (targetBlock == Config.TerminalBlock)
            {
                GameController.EmitEvent(this, $"core.dialog-cutscene.terminal-frame.{CurrentBlock.Id}");
            }

            else if (Blocks.ContainsKey(targetBlock))
            {
                LoadBlock(targetBlock);
            }

            else GameController.EmitEvent(this, $"core.dialog-cutscene.dialog-event.{targetBlock}");
        }

        else ProcessUpdate();
    }

    private void ProcessUpdate()
    {
        var finished = false;

        while (!finished)
        {
            var update = Updates.Dequeue();

            switch (update)
            {
                case GWSetTextUpdate textUpdate:
                    DialogBox.Text = textUpdate.Text;
                    finished = true;
                    break;
                case GWAddLeftCharacterUpdate addLeftUpdate:
                    AddCharacter(addLeftUpdate);
                    break;
                case GWAddRightCharacterUpdate addRightUpdate:
                    AddCharacter(addRightUpdate);
                    break;
                case GWRemoveLeftCharacterUpdate removeLeftUpdate:
                    RemoveCharacter(removeLeftUpdate);
                    break;
                case GWRemoveRightCharacterUpdate removeRightUpdate:
                    RemoveCharacter(removeRightUpdate);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }

    private void AddCharacter(GWCharacterUpdate update)
    {
        var character = Characters[update.Character];
        var container = LeftCharactersContainer;
        var cacheKey = $"left.{character.Id}";
        var sprite = new TextureRect
        {
            Texture = GD.Load<Texture2D>(character.SpritePath),
            ExpandMode = TextureRect.ExpandModeEnum.FitWidthProportional,
        };

        if (update is GWAddRightCharacterUpdate)
        {
            container = RightCharactersContainer;
            cacheKey = $"right.{character.Id}";
            sprite.FlipH = true;
        }

        CurrentCharacters[cacheKey] = sprite;
        container.AddChild(sprite);
    }

    private void RemoveCharacter(GWCharacterUpdate update)
    {
        var character = Characters[update.Character];
        var cacheKey = $"left.{character.Id}";

        if (update is GWRemoveRightCharacterUpdate)
            cacheKey = $"right.{character.Id}";

        CurrentCharacters[cacheKey].QueueFree();
        CurrentCharacters.Remove(cacheKey);
    }

}
