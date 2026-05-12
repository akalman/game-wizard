using System.Collections.Generic;
using System.Linq;
using GameWizard.Engine;
using GameWizard.Engine.Schema.Logic;
using GameWizard.Engine.Util;
using Godot;

namespace GameWizard.Core.DialogCutscene;

public partial class DialogCutsceneController : TemplateController<DialogConfig>
{
    [Export] public TextureRect Background { get; set; }

    [Export] public CenterContainer BannerContainer { get; set; }
    [Export] public TextureRect BannerBackground { get; set; }

    [Export] public MarginContainer CharacterMargin { get; set; }
    [Export] public HBoxContainer LeftCharactersContainer { get; set; }
    [Export] public HBoxContainer RightCharactersContainer { get; set; }

    [Export] public CenterContainer DialogBoxContainer { get; set; }
    [Export] public TextureRect DialogBoxBackground { get; set; }
    [Export] public MarginContainer DialogBoxMargin { get; set; }
    [Export] public RichTextLabel DialogBox { get; set; }

    private IDictionary<string, TextureRect> LoadedCharacters { get; } = new Dictionary<string, TextureRect>();
    private string CurrentShot { get; set; }
    private IList<IDialogFrame> RemainingFrames { get; set; } = new List<IDialogFrame>();

    protected override void InitializeScene()
    {
        StyleScene();
        LoadShot(Config.InitialShot);
    }

    public override bool HandleInput(string input)
    {
        switch (input)
        {
            case "advance":
                AdvanceFrame();
                return true;
            case "skip":
                break;
        }

        return false;
    }

    public override void HandleFocus(string sourceScene, string outputId)
    {
        // TODO: add logic to resume after action
    }

    private void StyleScene()
    {
        if (!Config.Style.Background.IsNullOrEmpty())
            Background.Texture = GD.Load<Texture2D>(Config.Style.Background);

        BannerContainer.CustomMinimumSize = Vector2.Down * Config.Style.Banner.Height;
        if (!Config.Style.Banner.Background.IsNullOrEmpty())
            BannerBackground.Texture = GD.Load<Texture2D>(Config.Style.Banner.Background);

        CharacterMargin.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        if (Config.Style.Characters.OuterMargin != 0)
        {
            CharacterMargin.AddThemeConstantOverride("margin_left", Config.Style.Characters.OuterMargin);
            CharacterMargin.AddThemeConstantOverride("margin_right", Config.Style.Characters.OuterMargin);
        }

        DialogBoxContainer.CustomMinimumSize = Vector2.Down * Config.Style.DialogBox.Height;
        DialogBoxMargin.CustomMinimumSize += Vector2.Down * Config.Style.DialogBox.Height;
        if (!Config.Style.DialogBox.Background.IsNullOrEmpty())
            DialogBoxBackground.Texture = GD.Load<Texture2D>(Config.Style.DialogBox.Background);
        if (Config.Style.DialogBox.TextMargin != Vector2.Zero)
        {
            DialogBoxMargin.AddThemeConstantOverride("margin_left", (int) Config.Style.DialogBox.TextMargin.X);
            DialogBoxMargin.AddThemeConstantOverride("margin_right", (int) Config.Style.DialogBox.TextMargin.X);
            DialogBoxMargin.AddThemeConstantOverride("margin_top", (int) Config.Style.DialogBox.TextMargin.Y);
            DialogBoxMargin.AddThemeConstantOverride("margin_bottom", (int) Config.Style.DialogBox.TextMargin.Y);
        }

    }

    private void LoadShot(string shotId)
    {
        CurrentShot = shotId;
        RemainingFrames = Config.Shots[shotId].Frames.ToList();
        AdvanceFrame();
    }

    private void AdvanceFrame()
    {
        if (RemainingFrames.IsEmpty())
        {
            var shot = Config.Shots[CurrentShot];

            switch (shot.EndAction.Type)
            {
                case EndActionType.End:
                    EmitOutput("terminal-frame", CurrentShot);
                    return;
                case EndActionType.SendAction:
                    EmitOutput("dialog-interaction", shot.EndAction.Destination);
                    return;
                case EndActionType.RollShot:
                    LoadShot(shot.EndAction.Destination);
                    return;
                default:
                    throw new GameWizardInternalException();
            }
        }

        var update = RemainingFrames[0];

        RemainingFrames.RemoveAt(0);

        ProcessFrame(update);

        if (update is not SetTextFrame)
            AdvanceFrame();
    }

    private void ProcessFrame(IDialogFrame frame)
    {
        switch (frame)
        {
            case AddCharacterFrame a:
                AddCharacter(a.Character, a.Side, a.Side);
                break;
            case SetTextFrame b:
                SetText(b.Text, b.Character);
                break;
            default:
                throw new GameWizardInternalException();
        }
    }

    public void AddCharacter(string characterId, HorizontalDirection screenSide, HorizontalDirection lineupSide)
    {
        var character = Config.Characters[characterId];

        var characterNode = new TextureRect
        {
            Texture = GD.Load<Texture2D>(character.Sprite),
            ExpandMode = TextureRect.ExpandModeEnum.FitWidthProportional,
            FlipH = screenSide switch
            {
                HorizontalDirection.Left => false,
                HorizontalDirection.Right => true,
                _ => throw new GameWizardInternalException($"Encountered unknown side: {screenSide}."),
            }
        };

        var container = screenSide switch
        {
            HorizontalDirection.Left => LeftCharactersContainer,
            HorizontalDirection.Right => RightCharactersContainer,
            _ => throw new GameWizardInternalException($"Encountered unknown side: {screenSide}"),
        };

        if (LoadedCharacters.ContainsKey(characterId))
            RemoveCharacter(characterId);

        LoadedCharacters[characterId] = characterNode;
        container.AddChild(characterNode);

        var targetIndex = lineupSide switch
        {
            HorizontalDirection.Left => 0,
            HorizontalDirection.Right => container.GetChildCount() - 1,
            _ => throw new GameWizardInternalException($"Encountered unknown side: {lineupSide}"),
        };
        container.MoveChild(characterNode, targetIndex);
    }

    public void RemoveCharacter(string characterId)
    {
        var characterNode = LoadedCharacters[characterId];

        LoadedCharacters.Remove(characterId);
        characterNode.QueueFree();
    }

    public void SetText(string text, string characterId)
    {
        DialogBox.Text = text;
    }
}