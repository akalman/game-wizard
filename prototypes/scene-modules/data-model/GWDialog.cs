namespace Prototypes.SceneModules.DataModel;

public class GWDialog : IGWScene
{
    public string DialogId { get; set; }
    public GWDialogFrame[] Frames { get; set; }
    public GWDialogCharacter[] Characters { get; set; }
    public string InitialFrameId { get; set; }
    public string TerminalEdge { get; set; }
}

public struct GWDialogFrame
{
    public string FrameId { get; set; }
    public GWDialogFrameComponentState State { get; set; }
    public GWDialogEdge[] Edges { get; set; }
}

public struct GWDialogFrameComponentState
{
    // TODO: implement top bar
    public string[] LeftCharacterIds { get; set; }
    public string[] RightCharacterIds { get; set; }
    public string DialogText { get; set; }
}

public struct GWDialogEdge
{
    public string Command { get; set; }
    public string DestinationFrame { get; set; }
}

public struct GWDialogCharacter
{
    public string CharacterId { get; set; }
    public string SpritePath { get; set; }
}
