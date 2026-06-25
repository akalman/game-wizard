using System.Collections.Generic;
using GameWizard.Engine.Schema.Logic;
using Godot;

namespace GameWizard.Core.LandmarkOverworld;

public class OverworldConfig
{
    public OverworldMap Map { get; set; }

    public IDictionary<string, OverworldLandmark> Landmarks { get; set; } = new Dictionary<string, OverworldLandmark>();
}

public class OverworldMap
{
    public string Sprite { get; set; }
    public MapScaling Scaling { get; set; }
}

public enum MapScaling
{
    ActualSize,
    FitWidth,
    FitHeight,
}

public class OverworldLandmark
{
    public string Sprite { get; set; }
    public Vector2 Size { get; set; }
    public Vector2 Offset { get; set; }

    public IList<ICondition> When { get; set; } = new List<ICondition>();
}