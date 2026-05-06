using System.Collections.Generic;

namespace GameWizard.Engine.Schema.Game;

public class GameConfig
{
    public string Name { get; set; }
    public GameState State { get; set; }
    public string InitialScene { get; set; }

    public IList<string> Modules { get; set; } = new List<string>();
    public IDictionary<string, GameScene> Scenes { get; set; } = new Dictionary<string, GameScene>();
}
