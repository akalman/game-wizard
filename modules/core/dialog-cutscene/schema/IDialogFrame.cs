using GameWizard.Engine.Schema.Logic;

namespace GameWizard.Core.DialogCutscene;

public interface IDialogFrame
{
}

public class AddCharacterFrame : IDialogFrame
{
    public HorizontalDirection Side { get; set; }
    public string Character { get; set; }
}

public class SetTextFrame : IDialogFrame
{
    public string Character { get; set; }
    public string Text { get; set; }
}