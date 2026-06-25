using System.Collections.Generic;

namespace GameWizard.Engine.Schema.Logic;

public interface ICondition;

public class FlagInCondition : ICondition
{
    public string FlagId { get; set; }
    public IList<string> AllowedValues { get; set; }
}

public class AttributeEqualsCondition : ICondition
{
    public string AttributeId { get; set; }
    public decimal ExpectedValue { get; set; }
}

public class AttributeMoreThanCondition : ICondition
{
    public string AttributeId { get; set; }
    public decimal Threshold { get; set; }
}

public class AttributeLessThanCondition : ICondition
{
    public string AttributeId { get; set; }
    public decimal Threshold { get; set; }
}

public class BagContainsMoreThanCondition : ICondition
{
    public string BagId { get; set; }
    public string ItemId { get; set; }
    public int Threshold { get; set; }
}

public class BagContainsLessThanCondition : ICondition
{
    public string BagId { get; set; }
    public string ItemId { get; set; }
    public int Threshold { get; set; }
}