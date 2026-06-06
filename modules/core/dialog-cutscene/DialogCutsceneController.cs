using System;
using System.Collections.Generic;
using System.Linq;
using GameWizard.Engine;
using GameWizard.Engine.Database;
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

    private IDictionary<string, (MarginContainer, HorizontalDirection)> LoadedCharacters { get; } =
        new Dictionary<string, (MarginContainer, HorizontalDirection)>();
    private IDictionary<string, IList<(MarginContainer, int)>> LoadedOutfits { get; } =
        new Dictionary<string, IList<(MarginContainer, int)>>();
    private string CurrentSequence { get; set; }
    private string CurrentInterlude { get; set; }
    private IList<IDialogFrame> RemainingFrames { get; set; } = new List<IDialogFrame>();

    private bool _animating;
    private bool Animating
    {
        get => _animating;
        set => _animating = value || !CurrentAnimations.IsEmpty();
    }

    private IDictionary<Guid, Tween> CurrentAnimations { get; set; } = new Dictionary<Guid, Tween>();

    protected override void InitializeScene()
    {
        StyleScene();
        LoadSequence(Config.InitialSequence);
    }

    public override bool HandleInput(IDictionary<string, bool> inputs)
    {
        var handled = false;

        if (inputs["advance"])
        {
            if (Animating)
                FlushAnimations();
            else
                AdvanceFrame();
            handled = true;
        }

        if (inputs["skip"])
        {
            // TODO: implement skip
        }

        return handled;
    }

    public override void HandleFocusUpdated(string sourceScene, string outputId, FocusState updatedState)
    {
        if (CurrentInterlude.IsNullOrEmpty() || updatedState != FocusState.Gained)
            return;

        if (!Config.Interludes.TryGetValue(CurrentInterlude, out var interlude))
            throw new InvalidDialogException($"Did not find definition for interlude: {CurrentInterlude}");

        var transition = interlude.Transitions
            .FirstOrDefault(trn => trn.Source == $"{outputId}" && trn.When.Evaluate(Game.State));
        if (transition is null)
            throw new InvalidDialogException($"Did not find transition for source {outputId}.");

        ProcessTransition(transition.Action);
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

    private void LoadSequence(string sequenceId)
    {
        CurrentSequence = sequenceId;
        RemainingFrames = Config.Sequences[sequenceId].Frames.ToList();
        AdvanceFrame();
    }

    private void AdvanceFrame()
    {
        if (RemainingFrames.IsEmpty())
        {
            var sequence = Config.Sequences[CurrentSequence];

            var transition = sequence.Transitions.FirstOrDefault(transition => transition.When.Evaluate(Game.State));
            if (transition is null)
                throw new InvalidDialogException($"Did not find an applicable transition for sequence: {CurrentSequence}.");

            ProcessTransition(transition.Action);
            return;
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
            case RemoveCharacterFrame b:
                RemoveCharacter(b.Character);
                break;
            case SetOutfitFrame c:
                SetOutfit(c.Character, c.Outfit);
                break;
            case SetTextFrame d:
                SetText(d.Text, d.Character);
                break;
            default:
                throw new GameWizardInternalException();
        }
    }

    private void AddCharacter(string characterId, HorizontalDirection screenSide, HorizontalDirection lineupSide)
    {
        Animating = true;
        var character = Game.Db.ReadDb(Config.Characters, characterId);

        var (container, targetIdx, flipH, slideProp) = screenSide switch
        {
            HorizontalDirection.Left => (LeftCharactersContainer, 0, false, "margin_left"),
            HorizontalDirection.Right => (RightCharactersContainer, RightCharactersContainer.GetChildCount() - 1, true, "margin_right"),
            _ => throw new GameWizardInternalException()
        };

        var characterContainer = new MarginContainer();
        var baseContainer = new MarginContainer { Name = "Base" };
        foreach (var spritePath in character.Get<IList<string>>("sprites"))
            baseContainer.AddChild(new TextureRect
            {
                Texture = GD.Load<Texture2D>(spritePath),
                ExpandMode = TextureRect.ExpandModeEnum.FitWidthProportional,
                FlipH = flipH,
            });

        var fadeId = Guid.NewGuid();
        var fadeTween = GetTree().CreateTween();
        CurrentAnimations[fadeId] = fadeTween;
        characterContainer.Modulate = new Color(1, 1, 1, 0);
        fadeTween.TweenProperty(characterContainer, "modulate", new Color(1, 1, 1, 1), 0.2f)
            .SetTrans(Tween.TransitionType.Linear)
            .SetEase(Tween.EaseType.InOut);
        fadeTween.TweenCallback(Callable.From(() =>
        {
            CurrentAnimations.Remove(fadeId);
            Animating = false;
        }));

        var slideId = Guid.NewGuid();
        var slideTween = GetTree().CreateTween();
        CurrentAnimations[slideId] = slideTween;
        slideTween.TweenMethod(
                Callable.From<int>(val => characterContainer.AddThemeConstantOverride(slideProp, val)),
                -50, 0, 0.2f)
            .SetTrans(Tween.TransitionType.Linear)
            .SetEase(Tween.EaseType.InOut);
        slideTween.TweenCallback(Callable.From(() =>
        {
            CurrentAnimations.Remove(slideId);
            Animating = false;
        }));

        if (LoadedCharacters.ContainsKey(characterId))
            RemoveCharacter(characterId);
        LoadedCharacters[characterId] = (characterContainer, screenSide);
        LoadedOutfits[characterId] = new List<(MarginContainer, int)>();

        characterContainer.AddChild(baseContainer);
        container.AddChild(characterContainer);
        container.MoveChild(characterContainer, targetIdx);
    }

    private void RemoveCharacter(string characterId)
    {
        var (node, side) = LoadedCharacters[characterId];

        LoadedCharacters.Remove(characterId);
        node.QueueFree();
    }

    private void SetOutfit(string characterId, string outfitId)
    {
        var character = Game.Db.ReadDb(Config.Characters, characterId);
        var outfit = Game.Db.ReadDb(Config.Outfits, outfitId);
        var (node, side) = LoadedCharacters[characterId];
        var flipH = side == HorizontalDirection.Right;
        var loadedOutfits = LoadedOutfits[characterId];
        var targetLayer = (int)outfit.Get<decimal>("layer");
        var outfitNode = CreateOutfit(outfit, targetLayer, flipH);

        if (loadedOutfits.IsEmpty())
        {
            loadedOutfits.Add((outfitNode, targetLayer));
            node.AddChild(outfitNode);
            return;
        }

        var currIdx = 0;
        while (currIdx < loadedOutfits.Count)
        {
            var (existingNode, layer) = loadedOutfits[currIdx];
            if (layer == targetLayer)
            {
                existingNode.QueueFree();
                loadedOutfits[currIdx] = (outfitNode, layer);
                node.AddChild(outfitNode);
                node.MoveChild(outfitNode, currIdx + 2);
                return;
            }

            if (layer > targetLayer)
            {
                break;
            }

            currIdx += 1;
        }

        loadedOutfits.Insert(currIdx, (outfitNode, targetLayer));
        node.AddChild(outfitNode);
        node.MoveChild(outfitNode, currIdx + 1);
    }

    private void SetText(string text, string characterId)
    {
        Animating = true;
        DialogBox.Text = text;
        DialogBox.VisibleCharacters = 0;

        var speakId = Guid.NewGuid();
        var speakTween = GetTree().CreateTween();
        CurrentAnimations[speakId] = speakTween;
        speakTween.TweenProperty(DialogBox, "visible_characters", text.Length, 0.05 * text.Length)
            .SetTrans(Tween.TransitionType.Linear)
            .SetEase(Tween.EaseType.InOut);
        speakTween.TweenCallback(Callable.From(() =>
        {
            CurrentAnimations.Remove(speakId);
            Animating = false;
        }));
    }

    private void ProcessTransition(TransitionAction action)
    {
        switch (action.Type)
        {
            case TransitionActionType.End:
                EmitOutput("terminal-frame", CurrentSequence);
                return;
            case TransitionActionType.StartInterlude:
                CurrentInterlude = action.Destination;
                EmitOutput("dialog-interlude", action.Destination);
                return;
            case TransitionActionType.StartSequence:
                LoadSequence(action.Destination);
                return;
            default:
                throw new GameWizardInternalException();
        }
    }

    private void FlushAnimations()
    {
        foreach (var (animationId, tween) in CurrentAnimations)
            tween.CustomStep(999999f);
    }

    private MarginContainer CreateOutfit(DatabaseEntry outfit, int targetLayer, bool flipH)
    {
        var outfitContainer = new MarginContainer { Name = $"outfit layer {targetLayer}" };
        foreach (var spritePath in outfit.Get<IList<string>>("sprites"))
            outfitContainer.AddChild(new TextureRect
            {
                Texture = GD.Load<Texture2D>(spritePath),
                ExpandMode = TextureRect.ExpandModeEnum.FitWidthProportional,
                FlipH = flipH,
            });
        return outfitContainer;
    }
}