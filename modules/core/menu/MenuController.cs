using GameWizard.Engine;
using GameWizard.Engine.Util;
using Godot;

namespace GameWizard.Core.Menu;

public partial class MenuController : TemplateController<MenuConfig>
{
    [Export] public TextureRect Background { get; set; }
    [Export] public VBoxContainer OptionsContainer { get; set; }

    [Export] public PackedScene MenuOptionScene { get; set; }

    private string CurrentPage { get; set; }

    protected override void InitializeScene()
    {
        if (!Config.Style.Background.IsNullOrEmpty())
            Background.Texture = GD.Load<Texture2D>(Config.Style.Background);

        LoadPage(Config.InitialPage);
    }

    public override bool HandleInput(string input)
    {
        return false;
    }

    public override void HandleFocus(string sourceScene, string outputId)
    {

    }

    private void LoadPage(string pageId)
    {
        foreach (var child in OptionsContainer.GetChildren())
            child.QueueFree();

        var page = Config.Pages.SafeGet(pageId);

        foreach (var (optionId, option) in page.Options)
        {
            var optionNode = (CenterContainer) MenuOptionScene.Instantiate();
            var buttonNode = (TextureButton) optionNode.FindChild("OptionButton");
            var labelNode = (RichTextLabel) optionNode.FindChild("OptionLabel");

            buttonNode.TextureNormal = GD.Load<Texture2D>(Config.Style.Options.Sprite);
            buttonNode.CustomMinimumSize = Config.Style.Options.Size;
            buttonNode.Pressed += () => HandleOptionPressed(optionId);

            labelNode.Text = option.Label;
            labelNode.CustomMinimumSize = Config.Style.Options.Size;

            OptionsContainer.AddChild(optionNode);
        }

        CurrentPage = pageId;
    }

    private void HandleOptionPressed(string optionId)
    {
        var page = Config.Pages[CurrentPage];
        var option = page.Options[optionId];

        switch (option.Action.Type)
        {
            case MenuActionType.LoadPage:
                LoadPage(option.Action.Destination);
                break;
            case MenuActionType.End:
                EmitOutput("terminal-select", optionId);
                break;
            case MenuActionType.None:
                break;
            default:
                throw new InvalidMenuException();
        }
    }
}
