using System.Collections.Generic;

namespace GameWizard.Core;

public class GWMenu
{
    public string MenuId { get; set; }
    public string InitialFrame { get; set; }
    public IList<GWMenuFrame> Frames { get; set; }
}