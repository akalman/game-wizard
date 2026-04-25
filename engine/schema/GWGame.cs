using System.Collections.Generic;

namespace GameWizard.Engine.Schema;

public class GWGame
{
    public IList<string> Modules { get; set; }
    public IList<GWScreen> Screens { get; set; }
    public IList<GWEdge> Edges { get; set; }
    public GWState State { get; set; }
    public string InitialScreen { get; set; }
}