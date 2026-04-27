using System.Collections.Generic;

namespace GameWizard.Engine.Schema;

public class GWCondition
{
    public GWConditionType Type { get; set; }
    public string Name { get; set; }
    public GWConditionComparator Comparator { get; set; }
    public IList<string> Targets { get; set; }
}