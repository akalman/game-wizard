using System.Collections.Generic;

namespace GameWizard.Engine.Schema;

public class GWEdge
{
    public string Source { get; set; }
    public string Event { get; set; }
    public string EventId { get; set; }
    public string Destination { get; set; }
    public IList<GWCondition> Conditions { get; set; } = new List<GWCondition>();
    public IList<GWStateChange> Changes { get; set; } = new List<GWStateChange>();
}