using System.Collections.Generic;

namespace GameWizard.Engine.Schema.Game;

public class GameState
{
    public IDictionary<string, StateFlag> Flags { get; set; } = new Dictionary<string, StateFlag>();
}

public class StateFlag
{
    public string InitialValue { get; set; }

    public IList<string> Values { get; set; } = new List<string>();
}

public class StateUpdate
{
    public StateUpdateType Type { get; set; }
    public string Name { get; set; }
    public string FlagValue { get; set; }
}

public enum StateUpdateType
{
    SetFlag,
}