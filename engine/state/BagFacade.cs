using System.Collections.Generic;
using GameWizard.Engine.Schema.Game;

namespace GameWizard.Engine.State;

public class BagFacade : StateFacade
{
    protected override IDictionary<StateUpdateType, StateFacadeUpdateFunc> GetUpdaters()
    {
        return new Dictionary<StateUpdateType, StateFacadeUpdateFunc>
        {
            { StateUpdateType.SetAmountInBag, SetAmountInBag },
            { StateUpdateType.AddToBag, AddToBag },
            { StateUpdateType.RemoveFromBag, RemoveFromBag },
            { StateUpdateType.ClearBag, ClearBag },
        };
    }

    public int GetNumInBag(GameState definition, SaveState state, string stateId, string bagItemId)
    {
        ValidateUpdateValid(definition, state, stateId);

        return state.Bags[stateId][bagItemId];
    }

    private void SetAmountInBag(GameState definition, SaveState state, StateUpdate update)
    {
        ValidateUpdateValid(definition, state, update.StateName);

        state.Bags[update.StateName][update.BagItemName] = (int) update.Number;
    }

    private void AddToBag(GameState definition, SaveState state, StateUpdate update)
    {
        ValidateUpdateValid(definition, state, update.StateName);

        var target = state.Bags[update.StateName][update.BagItemName] + (int) update.Number;
        ValidateUpdateInRange(update.StateName, target);

        state.Bags[update.StateName][update.BagItemName] = target;
    }

    private void RemoveFromBag(GameState definition, SaveState state, StateUpdate update)
    {
        ValidateUpdateValid(definition, state, update.StateName);

        var target = state.Bags[update.StateName][update.BagItemName] - (int) update.Number;
        ValidateUpdateInRange(update.StateName, target);

        state.Bags[update.StateName][update.BagItemName] = target;
    }

    private void ClearBag(GameState definition, SaveState state, StateUpdate update)
    {
        ValidateUpdateValid(definition, state, update.StateName);

        state.Bags[update.StateName].Clear();
    }

    private void ValidateUpdateValid(GameState definition, SaveState state, string stateId)
    {
        if (!definition.Bags.ContainsKey(stateId))
            throw new InvalidGameStateException($"Tried to read undefined bag {stateId}.");

        if (!state.Bags.ContainsKey(stateId))
            throw new GameWizardInternalException($"Could not find bag {stateId} in loaded state.");
    }

    private void ValidateUpdateInRange(string stateId, decimal target)
    {
        if (target < 0)
            throw new GameWizardInternalException();
    }
}