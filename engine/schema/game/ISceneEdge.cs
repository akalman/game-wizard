namespace GameWizard.Engine.Schema.Game;

public interface ISceneEdge
{
    public EdgeType Type { get; }
    public string OutputId { get; set; }
    public string OutputArg { get; set; }
}

public enum EdgeType
{
    ToSibling,
    ToChild,
    ToParent,
    ToSelf,
    Quit,
}

public class ToSiblingEdge : ISceneEdge
{
    public EdgeType Type => EdgeType.ToSibling;
    public string OutputId { get; set; }
    public string OutputArg { get; set; }
    public string Destination { get; set; }
}

public class ToChildEdge : ISceneEdge
{
    public EdgeType Type => EdgeType.ToChild;
    public string OutputId { get; set; }
    public string OutputArg { get; set; }
    public string Destination { get; set; }
}

public class ToParentEdge : ISceneEdge
{
    public EdgeType Type => EdgeType.ToParent;
    public string OutputId { get; set; }
    public string OutputArg { get; set; }
}

public class ToSelfEdge : ISceneEdge
{
    public EdgeType Type => EdgeType.ToSelf;
    public string OutputId { get; set; }
    public string OutputArg { get; set; }
}

public class QuitEdge : ISceneEdge
{
    public EdgeType Type => EdgeType.Quit;
    public string OutputId { get; set; }
    public string OutputArg { get; set; }
}