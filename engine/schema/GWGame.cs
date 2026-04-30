using System.Collections.Generic;

namespace GameWizard.Engine.Schema;

public class GWGame
{
    public IList<string> Modules { get; set; } = new List<string>();
    public IList<GWScreen> Screens { get; set; } = new List<GWScreen>();
    public IList<GWEdge> Edges { get; set; } = new List<GWEdge>();
    public GWState State { get; set; }
    public string InitialScreen { get; set; }
    public string CloseScreen { get; set; }
}