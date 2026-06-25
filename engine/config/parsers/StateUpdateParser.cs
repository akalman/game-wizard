using System.Collections.Generic;
using GameWizard.Engine.Schema.Game;

namespace GameWizard.Engine.Config;

public static class StateUpdateParser
{
    public static readonly IList<GrammarTypeMapper<IStateUpdate>> Mappers =
    [
        new SetFlagUpdateMapper(),
        new SetAttributeUpdateMapper(),
        new AddToAttributeUpdateMapper(),
        new SubtractFromAttributeUpdateMapper(),
        new SetAmountInBagUpdateMapper(),
        new AddToAmountInBagUpdateMapper(),
        new SubtractFromAmountInBagUpdateMapper(),
        new ClearFromBagUpdateMapper(),
    ];
}

public class SetFlagUpdateMapper : GrammarTypeMapper<SetFlagUpdate>
{
    public IDictionary<string, string> Grammar => new Dictionary<string, string>
    {
        ["set flag [id:flag]"] = "to [word:value]",
    };

    public SetFlagUpdate Map(IDictionary<string, string> captures) => new()
    {
        FlagId = captures["flag"],
        Value = captures["value"],
    };
}

public class SetAttributeUpdateMapper : GrammarTypeMapper<SetAttributeUpdate>
{
    public IDictionary<string, string> Grammar => new Dictionary<string, string>
    {
        ["set attribute [id:attribute]"] = "to [word:value]",
    };

    public SetAttributeUpdate Map(IDictionary<string, string> captures) => new()
    {
        AttributeId = captures["attribute"],
        Value = decimal.Parse(captures["value"]),
    };
}

public class AddToAttributeUpdateMapper : GrammarTypeMapper<AddToAttributeUpdate>
{
    public IDictionary<string, string> Grammar => new Dictionary<string, string>
    {
        ["raise [id:attribute]"] = "by [num:amount]",
    };

    public AddToAttributeUpdate Map(IDictionary<string, string> captures) => new()
    {
        AttributeId = captures["attribute"],
        Amount = decimal.Parse(captures["amount"]),
    };
}

public class SubtractFromAttributeUpdateMapper : GrammarTypeMapper<SubtractFromAttributeUpdate>
{
    public IDictionary<string, string> Grammar => new Dictionary<string, string>
    {
        ["lower [id:attribute]"] = "by [num:amount]",
    };

    public SubtractFromAttributeUpdate Map(IDictionary<string, string> captures) => new()
    {
        AttributeId = captures["attribute"],
        Amount = decimal.Parse(captures["amount"]),
    };
}

public class SetAmountInBagUpdateMapper : GrammarTypeMapper<SetAmountInBagUpdate>
{
    public IDictionary<string, string> Grammar => new Dictionary<string, string>
    {
        ["set [id:item] in bag [id:bag]"] = "to [num:value]",
    };

    public SetAmountInBagUpdate Map(IDictionary<string, string> captures) => new()
    {
        BagId = captures["bag"],
        ItemId = captures["item"],
        Value = int.Parse(captures["value"]),
    };
}

public class AddToAmountInBagUpdateMapper : GrammarTypeMapper<AddToAmountInBagUpdate>
{
    public IDictionary<string, string> Grammar => new Dictionary<string, string>
    {
        ["add [num:amount] [id:item]"] = "to bag [id:bag]",
    };

    public AddToAmountInBagUpdate Map(IDictionary<string, string> captures) => new()
    {
        BagId = captures["bag"],
        ItemId = captures["item"],
        Amount = int.Parse(captures["amount"]),
    };
}

public class SubtractFromAmountInBagUpdateMapper : GrammarTypeMapper<SubtractFromAmountInBagUpdate>
{
    public IDictionary<string, string> Grammar => new Dictionary<string, string>
    {
        ["remove [num:amount] [id:item]"] = "from bag [id:bag]",
    };

    public SubtractFromAmountInBagUpdate Map(IDictionary<string, string> captures) => new()
    {
        BagId = captures["bag"],
        ItemId = captures["item"],
        Amount = int.Parse(captures["amount"]),
    };
}

public class ClearFromBagUpdateMapper : GrammarTypeMapper<ClearFromBagUpdate>
{
    public IDictionary<string, string> Grammar => new Dictionary<string, string>
    {
        ["clear all [id:item]"] = "from [id:bag]",
    };

    public ClearFromBagUpdate Map(IDictionary<string, string> captures) => new()
    {
        BagId = captures["bag"],
        ItemId = captures["item"],
    };
}
