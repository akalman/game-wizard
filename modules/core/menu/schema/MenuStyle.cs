using Godot;

namespace GameWizard.Core.Menu;

public class MenuStyle
{
    public string Background { get; set; }
    public PageStyle Page { get; set; }
    public OptionStyle Options { get; set; }
}

public class PageStyle
{

}

public class OptionStyle
{
    public string Sprite { get; set; }
    public Vector2 Size { get; set; }
}