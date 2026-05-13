using System.Collections.Generic;
using GameWizard.Engine.Schema.Logic;

namespace GameWizard.Engine.Schema.Game;

public class GameScene
{
    public string Template { get; set; }
    public string Config { get; set; }
    public bool AlwaysActive { get; set; }

    public IList<SceneTransition> Transitions { get; set; } = new List<SceneTransition>();
}

public class SceneTransition
{
    public Edge Edge { get; set; }

    public IList<Condition> When { get; set; } = new List<Condition>();
    public IList<StateUpdate> Updates { get; set; } = new List<StateUpdate>();
}

public class Edge
{
    public string OutputId { get; set; }
    public string OutputArg { get; set; }
    public EdgeType Type { get; set; }
    public string Destination { get; set; }
}

public enum EdgeType
{
    ToSibling,
    ToChild,
    ToParent,
    ToSelf,

    Quit,
}