using System.Collections.Generic;

namespace GameWizard.Engine.Schema;

public class GWModule
{
    public string ModuleId { get; set; }
    public IList<GWTemplate> Templates { get; set; }
}