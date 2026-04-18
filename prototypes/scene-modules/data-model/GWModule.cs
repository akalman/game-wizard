using System.Collections.Generic;
using Godot;

namespace Prototypes.SceneModules.DataModel;

public struct GWModule
{
    public string ModuleId { get; set; }
    public GWModuleScene[] Scenes { get; set; }
}

public struct GWModuleScene
{
    public string ModuleSceneId { get; set; }
    public string ModuleScenePath { get; set; }
    public string[] ModuleSceneCommands { get; set; }
}
