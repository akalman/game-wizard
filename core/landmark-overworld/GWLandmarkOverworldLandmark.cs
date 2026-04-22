using Godot;

namespace GameWizard.Core;

public struct GWLandmarkOverworldLandmark
{
    public string LandmarkId { get; set; }
    public string TexturePath { get; set; }
    public string Edge { get; set; }
    public Vector2 Size { get; set; }
    public Vector2 Offset { get; set; }
}