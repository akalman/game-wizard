using System.Collections.Generic;

namespace GameWizard.Engine.Schema;

public class GWModule
{
    public string Id { get; set; }
    public IList<GWTemplate> Templates { get; set; } = new List<GWTemplate>();
}