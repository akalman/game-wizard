namespace Prototypes.SceneModules.DataModel;

public struct GWGame
{
    public string[] Modules { get; set; }
    public GWGameState[] Scenes { get; set; }
    public GWGameEdge[] Edges { get; set; }
    public string InitialScene { get; set; }
}

public struct GWGameState
{
    public string StateId { get; set; }
    public string Template { get; set; }
    public string ConfigPath { get; set; }
    public bool IsOverlay { get; set; }
}

public struct GWGameEdge
{
    public string SourceScene { get; set; }
    public string EdgeName { get; set; }
    public string DestinationScene { get; set; }
}
