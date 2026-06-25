using System.Collections.Generic;

namespace GameWizard.Engine.Schema.Game;

public class GameState
{
    public IDictionary<string, StateFlag> Flags { get; set; } = new Dictionary<string, StateFlag>();
    public IDictionary<string, StateAttribute> Attributes { get; set; } = new Dictionary<string, StateAttribute>();
    public IDictionary<string, StateBag> Bags { get; set; } = new Dictionary<string, StateBag>();
}

public class StateFlag
{
    public string InitialValue { get; set; }

    public IList<string> Values { get; set; } = new List<string>();
}

public class StateAttribute
{
    public int Min { get; set; }
    public int Max { get; set; }
    public decimal InitialValue { get; set; }
}

public class StateBag
{
    public string Db { get; set; }
}
