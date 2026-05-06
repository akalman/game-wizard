using System.Collections.Generic;
using GameWizard.Engine.Schema.Game;

namespace GameWizard.Engine.State;

public class StateRepository : IStateRepository
{
    private GameState Definition { get; set; }
    public bool IsInitialized { get; set; } = false;

    private SaveState Current { get; set; }
    private bool IsLoaded { get; set; } = false;

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

        IsLoaded = true;
    }

    public void Load(string path)
    {
        throw new System.NotImplementedException();
    }

    public string ReadFlag(string flagId)
    {
        if (!IsInitialized)
            throw new GameWizardInternalException($"Tried to read flag {flagId} when state was uninitialized.");

        if (!IsLoaded)
            throw new InvalidGameStateException($"Tried to read flag {flagId} when state was unloaded.");

        if (!Definition.Flags.ContainsKey(flagId))
            throw new InvalidGameStateException($"Tried to read undefined flag {flagId}.");

        if (!Current.Flags.ContainsKey(flagId))
            throw new GameWizardInternalException($"Could not find flag {flagId} in loaded state.");

        return Current.Flags[flagId];
    }

    public void Update(StateUpdate update)
    {
        if (!IsInitialized)
            throw new GameWizardInternalException($"Tried to read state {update.Name} when state was uninitialized.");

        if (!IsLoaded)
            throw new InvalidGameStateException($"Tried to read state {update.Name} when state was unloaded.");

        switch (update.Type)
        {
            case StateUpdateType.SetFlag:
                if (!Definition.Flags.ContainsKey(update.Name))
                    throw new InvalidGameStateException($"Tried to read undefined flag {update.Name}.");

                if (!Definition.Flags[update.Name].Values.Contains(update.FlagValue))
                    throw new GameWizardInternalException($"Could not find flag {update.Name} in loaded state.");

                if (!Current.Flags.ContainsKey(update.Name))
                    throw new GameWizardInternalException($"Could not find flag {update.Name} in loaded state.");

                Current.Flags[update.Name] = update.FlagValue;
                break;
        }
    }

    private class SaveState
    {
        public IDictionary<string, string> Flags { get; set; } = new Dictionary<string, string>();
    }
}

