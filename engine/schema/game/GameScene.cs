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
    public ISceneEdge Edge { get; set; }

    public IList<ICondition> When { get; set; } = new List<ICondition>();
    public IList<IStateUpdate> Updates { get; set; } = new List<IStateUpdate>();
}