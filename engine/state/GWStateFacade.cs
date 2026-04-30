using System;
using System.Collections.Generic;
using System.Linq;
using GameWizard.Engine.Schema;
using Godot;

namespace GameWizard.Engine.State;

public class GWStateFacade
{
    public IList<string> FlagOrder { get; set; } = new List<string>();
    public IDictionary<string, GWStateFlag> FlagDefinitions { get; set; } = new Dictionary<string, GWStateFlag>();
    public IDictionary<string, string> Flags { get; set; } = new Dictionary<string, string>();
    public bool IsStateLoaded { get; set; } = false;

    public delegate void GWStateUpdatedHandler();

    public event GWStateUpdatedHandler StateUpdated;

    public GWStateFacade(GWState stateDefinition)
    {
        foreach (var flag in stateDefinition.Flags)
        {
            FlagOrder.Add(flag.Id);
            FlagDefinitions[flag.Id] = flag;
        }

        FlagOrder = FlagOrder.Order().ToList();
    }

    public void Create()
    {
        ClearLoadedState();

        foreach (var flagId in FlagOrder)
        {
            var flag = FlagDefinitions[flagId];
            Flags[flag.Id] = flag.InitialValue;
        }

        IsStateLoaded = true;
    }

    public void Load()
    {
        throw new NotImplementedException();
    }

    public void Save()
    {
        throw new NotImplementedException();
    }

    public string Read(string name)
    {
        if (!IsStateLoaded)
        {
            GD.PushError($"Tried to read state {name} when state has not been loaded.");
            throw new NotImplementedException();
        }

        switch (GetStateType(name))
        {
            case "flag":
                GD.PushWarning(name);
                return Flags[name];
            default:
                throw new NotImplementedException();
        }
    }

    public void Write(string name, string value)
    {
        if (!IsStateLoaded)
        {
            GD.PushError($"Tried to write state {name} when state has not been loaded.");
            throw new NotImplementedException();
        }

        switch (GetStateType(name))
        {
            case "flag":
                Flags[name] = value;
                break;
            default:
                throw new NotImplementedException();
        }

        StateUpdated?.Invoke();
    }

    private void ClearLoadedState()
    {
        if (IsStateLoaded)
        {
            Flags.Clear();
            IsStateLoaded = false;
        }
    }

    private string GetStateType(string name) => "flag";
}
