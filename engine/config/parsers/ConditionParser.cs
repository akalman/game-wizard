using System.Collections.Generic;
using GameWizard.Engine.Schema.Logic;

namespace GameWizard.Engine.Config;

public static class ConditionParser
{
    public static readonly IList<GrammarTypeMapper<ICondition>> Mappers = [
        new FlagInConditionMapper(),
        new AttributeEqualsConditionMapper(),
        new AttributeMoreThanConditionMapper(),
        new AttributeLessThanConditionMapper(),
        new BagContainsMoreThanConditionMapper(),
        new BagContainsLessThanConditionMapper()
    ];
}

public class FlagInConditionMapper : GrammarTypeMapper<FlagInCondition>
{
    public IDictionary<string, string> Grammar => new Dictionary<string, string>
    {
        ["flag [id:flag]"] = "in [id-list:values]",
    };

    public FlagInCondition Map(IDictionary<string, string> captures) => new()
    {
        FlagId = captures["flag"],
        AllowedValues = captures["values"].Split(","),
    };
}

public class AttributeEqualsConditionMapper : GrammarTypeMapper<AttributeEqualsCondition>
{
    public IDictionary<string, string> Grammar => new Dictionary<string, string>
    {
        ["attribute [id:attribute]"] = "is [num:expected]",
    };

    public AttributeEqualsCondition Map(IDictionary<string, string> captures) => new()
    {
        AttributeId = captures["attribute"],
        ExpectedValue = decimal.Parse(captures["expected"]),
    };
}

public class AttributeMoreThanConditionMapper : GrammarTypeMapper<AttributeMoreThanCondition>
{
    public IDictionary<string, string> Grammar => new Dictionary<string, string>
    {
        ["attribute [id:attribute]"] = "more than [num:expected]",
    };

    public AttributeMoreThanCondition Map(IDictionary<string, string> captures) => new()
    {
        AttributeId = captures["attribute"],
        Threshold = decimal.Parse(captures["expected"]),
    };
}

public class AttributeLessThanConditionMapper : GrammarTypeMapper<AttributeLessThanCondition>
{
    public IDictionary<string, string> Grammar => new Dictionary<string, string>
    {
        ["attribute [id:attribute]"] = "less than [num:expected]",
    };

    public AttributeLessThanCondition Map(IDictionary<string, string> captures) => new()
    {
        AttributeId = captures["attribute"],
        Threshold = decimal.Parse(captures["expected"]),
    };
}

public class BagContainsMoreThanConditionMapper : GrammarTypeMapper<BagContainsMoreThanCondition>
{
    public IDictionary<string, string> Grammar => new Dictionary<string, string>
    {
        ["bag [id:bag]"] = "more than [num:threshold] [id:item]",
    };

    public BagContainsMoreThanCondition Map(IDictionary<string, string> captures) => new()
    {
        BagId = captures["bag"],
        ItemId = captures["item"],
        Threshold = int.Parse(captures["threshold"]),
    };
}

public class BagContainsLessThanConditionMapper : GrammarTypeMapper<BagContainsLessThanCondition>
{
    public IDictionary<string, string> Grammar => new Dictionary<string, string>
    {
        ["bag [id:bag]"] = "less than [num:threshold] [id:item]",
    };

    public BagContainsLessThanCondition Map(IDictionary<string, string> captures) => new()
    {
        BagId = captures["bag"],
        ItemId = captures["item"],
        Threshold = int.Parse(captures["threshold"]),
    };
}