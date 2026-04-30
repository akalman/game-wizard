using System.Collections.Generic;

namespace GameWizard.Engine.Schema;

public class GWCondition
{
    public GWStatePredicate Predicate { get; set; }
    public string Target { get; set; }
    public IList<string> Set { get; set; }
}