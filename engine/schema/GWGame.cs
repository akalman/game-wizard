using System.Collections.Generic;

namespace GameWizard.Engine.Schema;

public class GWGame
{
    public IList<string> Modules { get; set; }
    public IList<GWState> States { get; set; }
    public IList<GWEdge> Edges { get; set; }
    public string InitialState { get; set; }
}
