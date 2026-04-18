using System.Collections.Generic;
using Godot;
using static Godot.TextureRect;

namespace Prototypes.SceneModules.DataModel;

public class GWOverworld : IGWScene
{
    public string OverworldId { get; set; }
    public GWOverworldMap Map { get; set; }
    public GWLandmark[] Landmarks { get; set; }
}

public struct GWOverworldMap
{
    public string MapTexturePath { get; set; }
    public GWMapScaling ScalingType { get; set; }
}

public enum GWMapScaling
{
    FitWidth,
    FitHeight,
    ActualSize,
}

public struct GWLandmark
{
    public string LandmarkId { get; set; }
    public string TexturePath { get; set; }
    public string Destination { get; set; }
    public Vector2 Size { get; set; }
    public Vector2 Offset { get; set; }
}

public static class GWOverworldExt
{
    public static IDictionary<GWMapScaling, ExpandModeEnum> Mapping => new Dictionary<GWMapScaling, ExpandModeEnum>
    {
        { GWMapScaling.FitWidth, ExpandModeEnum.FitHeightProportional },
        { GWMapScaling.FitHeight, ExpandModeEnum.FitWidthProportional },
        { GWMapScaling.ActualSize, ExpandModeEnum.KeepSize },
    };

    public static ExpandModeEnum ToExpandMode(this GWMapScaling scaling)
    {
        if (!Mapping.ContainsKey(scaling)) return ExpandModeEnum.IgnoreSize;
        return Mapping[scaling];
    }
}
