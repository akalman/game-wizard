using System.Collections.Generic;

namespace GameWizard.Core;

public class GWMenuFrame
{
    public string Id { get; set; }
    public IList<GWMenuOption> Options { get; set; }
}