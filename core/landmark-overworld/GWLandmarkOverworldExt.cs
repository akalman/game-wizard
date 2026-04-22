using System.Collections.Generic;
using ExpandMode = Godot.TextureRect.ExpandModeEnum;
using MapScaling = GameWizard.Core.GWLandmarkOverworldMapScaling;

namespace GameWizard.Core;

public static class GWLandmarkOverworldExt
{
    public static IDictionary<MapScaling, ExpandMode> Mapping => new Dictionary<MapScaling, ExpandMode>
    {
        { MapScaling.FitWidth, ExpandMode.FitHeightProportional },
        { MapScaling.FitHeight, ExpandMode.FitWidthProportional },
        { MapScaling.ActualSize, ExpandMode.KeepSize },
    };

    public static ExpandMode ToExpandMode(this MapScaling scaling)
    {
        if (!Mapping.ContainsKey(scaling)) return ExpandMode.IgnoreSize;
        return Mapping[scaling];
    }
}