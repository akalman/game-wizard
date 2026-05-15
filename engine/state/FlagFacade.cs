using System.Collections;
using System.Collections.Generic;
using GameWizard.Engine.Schema.Game;

namespace GameWizard.Engine.State;

public class FlagFacade : StateFacade
{
    protected override IDictionary<StateUpdateType, StateFacadeUpdateFunc> GetUpdaters()
    {
        return new Dictionary<StateUpdateType, StateFacadeUpdateFunc>
        {
            { StateUpdateType.SetFlag, SetFlag },
        };
    }

    public string GetFlag(GameState definition, SaveState state, string flagId)
    {
        if (!definition.Flags.ContainsKey(flagId))
            throw new InvalidGameStateException($"Tried to read undefined flag {flagId}.");

        if (!state.Flags.ContainsKey(flagId))
            throw new GameWizardInternalException($"Could not find flag {flagId} in loaded state.");

        return state.Flags[flagId];
    }

    private void SetFlag(GameState definition, SaveState state, StateUpdate update)
    {
        if (!definition.Flags.ContainsKey(update.StateName))
            throw new InvalidGameStateException($"Tried to read undefined flag {update.StateName}.");

        if (!definition.Flags[update.StateName].Values.Contains(update.String))
            throw new GameWizardInternalException($"Could not find flag {update.StateName} in loaded state.");

        if (!state.Flags.ContainsKey(update.StateName))
            throw new GameWizardInternalException($"Could not find flag {update.StateName} in loaded state.");

        state.Flags[update.StateName] = update.String;
    }
}