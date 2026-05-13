using GameWizard.Engine.Config;
using Godot;

namespace GameWizard.Engine;

public abstract partial class PluginController : Node2D
{
    public virtual void RegisterDeserializer<T>(IConfigLoader<T> loader) { }
}