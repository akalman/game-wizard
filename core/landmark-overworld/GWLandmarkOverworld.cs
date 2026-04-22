using System.Collections.Generic;

namespace GameWizard.Core;

public class GWLandmarkOverworld
{
    public string OverworldId { get; set; }
    public GWLandmarkOverworldMap Map { get; set; }
    public IList<GWLandmarkOverworldLandmark> Landmarks { get; set; }
}