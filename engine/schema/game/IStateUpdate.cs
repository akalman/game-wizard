namespace GameWizard.Engine.Schema.Game;

public interface IStateUpdate;

public class SetFlagUpdate : IStateUpdate
{
    public string FlagId { get; set; }
    public string Value { get; set; }
}

public class SetAttributeUpdate : IStateUpdate
{
    public string AttributeId { get; set; }
    public decimal Value { get; set; }
}

public class AddToAttributeUpdate : IStateUpdate
{
    public string AttributeId { get; set; }
    public decimal Amount { get; set; }
}

public class SubtractFromAttributeUpdate : IStateUpdate
{
    public string AttributeId { get; set; }
    public decimal Amount { get; set; }
}

public class SetAmountInBagUpdate : IStateUpdate
{
    public string BagId { get; set; }
    public string ItemId { get; set; }
    public int Value { get; set; }
}

public class AddToAmountInBagUpdate : IStateUpdate
{
    public string BagId { get; set; }
    public string ItemId { get; set; }
    public int Amount { get; set; }
}

public class SubtractFromAmountInBagUpdate : IStateUpdate
{
    public string BagId { get; set; }
    public string ItemId { get; set; }
    public int Amount { get; set; }
}

public class ClearFromBagUpdate : IStateUpdate
{
    public string BagId { get; set; }
    public string ItemId { get; set; }
}