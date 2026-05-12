using Godot;

namespace GameWizard.Core.DialogCutscene;

public class DialogStyle
{
    public string Background { get; set; }
    public BannerStyle Banner { get; set; }
    public CharactersStyle Characters { get; set; }
    public DialogBoxStyle DialogBox { get; set; }
}

public class BannerStyle
{
    public string Background { get; set; }
    public int Height { get; set; }
}

public class CharactersStyle
{
    public int OuterMargin { get; set; }
}

public class DialogBoxStyle
{
    public string Background { get; set; }
    public int Height { get; set; }
    public Vector2 TextMargin { get; set; }
}