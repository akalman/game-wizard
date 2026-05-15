using System.Collections.Generic;

namespace GameWizard.Engine.Schema.Logic;

public class Condition
{
    public ConditionType Type { get; set; }
    public string Target { get; set; }

    public IList<string> ExpectedMembership { get; set; } = new List<string>();
}

public enum ConditionType
{
    FlagIn,

    AttributeIs,
    AttributeMoreThan,
    AttributeLessThan,

    BagContains,
}