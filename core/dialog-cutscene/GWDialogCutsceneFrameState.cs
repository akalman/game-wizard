using System.Collections.Generic;

namespace GameWizard.Core;

public class GWDialogCutsceneFrameState
{
    // TODO: implement top bar
    public IList<string> LeftCharacterIds { get; set; }
    public IList<string> RightCharacterIds { get; set; }
    public string DialogText { get; set; }
}