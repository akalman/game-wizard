using System.Collections.Generic;

namespace GameWizard.Core;

public class GWDialogCutscene
{
    public string DialogId { get; set; }
    public IList<GWDialogCutsceneFrame> Frames { get; set; }
    public IList<GWDialogCutsceneCharacter> Characters { get; set; }
    public string InitialFrameId { get; set; }
    public string CompleteEdge { get; set; }
}