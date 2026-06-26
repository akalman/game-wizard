using System.Collections.Generic;
using GameWizard.Engine.Schema.Game;

namespace GameWizard.Engine.Config;

public static class SceneEdgeParser
{
    public static readonly IList<GrammarTypeMapper<ISceneEdge>> Mappers =
    [
        new ToSiblingEdgeMapper(),
        new ToChildEdgeMapper(),
        new ToParentEdgeMapper(),
        new ToSelfEdgeMapper(),
        new QuitEdgeMapper(),
    ];
}

public class ToSiblingEdgeMapper : GrammarTypeMapper<ToSiblingEdge>
{
    public IDictionary<string, string> Grammar => new Dictionary<string, string>
    {
        [string.Empty] = "[id:output] move to [id:destination]",
    };

    public ToSiblingEdge Map(IDictionary<string, string> captures) => new()
    {
        OutputId = captures["output"].Split(".")[0],
        OutputArg = captures["output"].Split(".")[1],
        Destination = captures["destination"],
    };
}

public class ToChildEdgeMapper : GrammarTypeMapper<ToChildEdge>
{
    public IDictionary<string, string> Grammar => new Dictionary<string, string>
    {
        [string.Empty] = "[id:output] spawn [id:destination]",
    };

    public ToChildEdge Map(IDictionary<string, string> captures) => new()
    {
        OutputId = captures["output"].Split(".")[0],
        OutputArg = captures["output"].Split(".")[1],
        Destination = captures["destination"],
    };
}

public class ToParentEdgeMapper : GrammarTypeMapper<ToParentEdge>
{
    public IDictionary<string, string> Grammar => new Dictionary<string, string>
    {
        [string.Empty] = "[id:output] ends",
    };

    public ToParentEdge Map(IDictionary<string, string> captures) => new()
    {
        OutputId = captures["output"].Split(".")[0],
        OutputArg = captures["output"].Split(".")[1],
    };
}

public class ToSelfEdgeMapper : GrammarTypeMapper<ToSelfEdge>
{
    public IDictionary<string, string> Grammar => new Dictionary<string, string>
    {
        [string.Empty] = "[id:output] does nothing",
    };

    public ToSelfEdge Map(IDictionary<string, string> captures) => new()
    {
        OutputId = captures["output"].Split(".")[0],
        OutputArg = captures["output"].Split(".")[1],
    };
}

public class QuitEdgeMapper : GrammarTypeMapper<QuitEdge>
{
    public IDictionary<string, string> Grammar => new Dictionary<string, string>
    {
        [string.Empty] = "[id:output] quits",
    };

    public QuitEdge Map(IDictionary<string, string> captures) => new()
    {
        OutputId = captures["output"].Split(".")[0],
        OutputArg = captures["output"].Split(".")[1],
    };
}