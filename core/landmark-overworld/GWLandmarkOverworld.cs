using System.Collections.Generic;
using GameWizard.Engine.Schema;
using Godot;

namespace GameWizard.Core;

public class GWLandmarkOverworld
{
    public string OverworldId { get; set; }
    public GWLandmarkOverworldMap Map { get; set; }
    public IList<GWLandmarkOverworldLandmark> Landmarks { get; set; }
}

public class GWLandmarkOverworldMap
{
    public string TexturePath { get; set; }
    public GWLandmarkOverworldMapScaling ScalingType { get; set; }
}

public class GWLandmarkOverworldLandmark
{
    public string Id { get; set; }
    public string Texture { get; set; }
    public string EventId { get; set; }
    public Vector2 Size { get; set; }
    public Vector2 Offset { get; set; }
    public IList<GWCondition> Conditions { get; set; }
}

public enum GWLandmarkOverworldMapScaling
{
    FitWidth,
    FitHeight,
    ActualSize,
}