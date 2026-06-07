using System.Collections.Generic;
using GameWizard.Engine.Schema.Game;

namespace GameWizard.Engine.State;

public class StateRepository : IStateRepository
{
    private FlagFacade Flags { get; set; } = new();
    private AttributeFacade Attributes { get; set; } = new();
    private BagFacade Bags { get; set; } = new();

    private GameState Definition { get; set; }
    public bool IsInitialized { get; set; }

    private SaveState Current { get; set; }
    private bool IsLoaded { get; set; }

    public void Initialize(GameState definition)
    {
        Definition = definition;
        IsInitialized = true;
    }

    public void Create()
    {
        if (!IsInitialized)
            throw new GameWizardInternalException($"Tried to create a game state when state was uninitialized.");

        Current = new SaveState();

        foreach (var (flagId, flag) in Definition.Flags)
            Current.Flags[flagId] = flag.InitialValue;

        foreach (var (attributeId, attribute) in Definition.Attributes)
            Current.Attributes[attributeId] = attribute.InitialValue;

        foreach (var (bagId, bag) in Definition.Bags)
            Current.Bags[bagId] = new Dictionary<string, int>();

        IsLoaded = true;
    }

    public void Load(string path)
    {
        throw new System.NotImplementedException();
    }

    public string ReadFlag(string stateId)
    {
        if (!IsInitialized)
            throw new GameWizardInternalException($"Tried to read flag {stateId} when state was uninitialized.");

        if (!IsLoaded)
            throw new InvalidGameStateException($"Tried to read flag {stateId} when state was unloaded.");

        return Flags.GetFlag(Definition, Current, stateId);
    }

    public decimal ReadAttribute(string stateId)
    {
        return Attributes.GetAttribute(Definition, Current, stateId);
    }

    public int NumInBag(string stateId, string itemId)
    {
        return Bags.GetNumInBag(Definition, Current, stateId, itemId);
    }

    public void Update(StateUpdate update)
    {
        if (!IsInitialized)
            throw new GameWizardInternalException($"Tried to read state {update.StateName} when state was uninitialized.");

        if (!IsLoaded)
            throw new InvalidGameStateException($"Tried to read state {update.StateName} when state was unloaded.");

        switch (update.Type)
        {
            case var flag when Flags.Accepts(flag):
                Flags.Update(Definition, Current, update);
                break;
            case var attribute when Attributes.Accepts(attribute):
                Attributes.Update(Definition, Current, update);
                break;
            case var bag when Bags.Accepts(bag):
                Bags.Update(Definition, Current, update);
                break;
        }
    }
}