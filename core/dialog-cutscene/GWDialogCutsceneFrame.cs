using System.Collections.Generic;

namespace GameWizard.Core;

public class GWDialogCutsceneFrame
{
    public string FrameId { get; set; }
    public GWDialogCutsceneFrameState State { get; set; }
    public IList<GWDialogCutsceneFrameEdge> Edges { get; set; }
}