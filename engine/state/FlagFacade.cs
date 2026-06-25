using GameWizard.Engine.Schema.Game;

namespace GameWizard.Engine.State;

public class FlagFacade : StateFacade
{
    public FlagFacade()
    {
        RegisterUpdater<SetFlagUpdate>(SetFlag);
    }

    public string GetFlag(GameState definition, SaveState state, string flagId)
    {
        if (!definition.Flags.ContainsKey(flagId))
            throw new InvalidGameStateException($"Tried to read undefined flag {flagId}.");

        if (!state.Flags.ContainsKey(flagId))
            throw new GameWizardInternalException($"Could not find flag {flagId} in loaded state.");

        return state.Flags[flagId];
    }

    private void SetFlag(GameState definition, SaveState state, SetFlagUpdate update)
    {
        if (!definition.Flags.ContainsKey(update.FlagId))
            throw new InvalidGameStateException($"Tried to read undefined flag {update.FlagId}.");

        if (!definition.Flags[update.FlagId].Values.Contains(update.Value))
            throw new GameWizardInternalException($"Could not find flag {update.FlagId} in loaded state.");

        if (!state.Flags.ContainsKey(update.FlagId))
            throw new GameWizardInternalException($"Could not find flag {update.FlagId} in loaded state.");

        state.Flags[update.FlagId] = update.Value;
    }
}