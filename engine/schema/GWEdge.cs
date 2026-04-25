using System.Collections.Generic;

namespace GameWizard.Engine.Schema;

public class GWEdge
{
    public string Source { get; set; }
    public string Type { get; set; }
    public string Edge { get; set; }
    public string Destination { get; set; }
    public IList<GWCondition> Conditions { get; set; }
    public IList<GWEdgeEffect> Effects { get; set; } = new List<GWEdgeEffect>();
}