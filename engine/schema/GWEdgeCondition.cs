namespace GameWizard.Engine.Schema;

public class GWEdgeCondition
{
    public GWEdgeConditionType Type { get; set; }
    public string Name { get; set; }
    public GWEdgeConditionComparator Comparator { get; set; }
    public string Target { get; set; }
}