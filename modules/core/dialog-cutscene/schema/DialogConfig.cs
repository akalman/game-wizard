using System.Collections.Generic;

namespace GameWizard.Core.DialogCutscene;

public class DialogConfig
{
    public string InitialShot { get; set; }
    public DialogStyle Style { get; set; }

    public IDictionary<string, DialogCharacter> Characters { get; set; } = new Dictionary<string, DialogCharacter>();
    public IDictionary<string, DialogShot> Shots { get; set; } = new Dictionary<string, DialogShot>();
}

public class DialogCharacter
{
    public string Sprite { get; set; }
}

public class DialogShot
{
    public ShotEndAction EndAction { get; set; }

    public IList<IDialogFrame> Frames { get; set; } = new List<IDialogFrame>();
}

public class ShotEndAction
{
    public EndActionType Type { get; set; }
    public string Destination { get; set; }
}

public enum EndActionType
{
    RollShot,
    SendAction,
    End,
}