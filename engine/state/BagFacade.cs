using GameWizard.Engine.Schema.Game;

namespace GameWizard.Engine.State;

public class BagFacade : StateFacade
{
    public BagFacade()
    {
        RegisterUpdater<SetAmountInBagUpdate>(SetAmountInBag);
        RegisterUpdater<AddToAmountInBagUpdate>(AddToBag);
        RegisterUpdater<SubtractFromAmountInBagUpdate>(RemoveFromBag);
        RegisterUpdater<ClearFromBagUpdate>(ClearBag);
    }

    public int GetNumInBag(GameState definition, SaveState state, string stateId, string bagItemId)
    {
        ValidateUpdateValid(definition, state, stateId, bagItemId);

        return state.Bags[stateId][bagItemId];
    }

    private void SetAmountInBag(GameState definition, SaveState state, SetAmountInBagUpdate update)
    {
        ValidateUpdateValid(definition, state, update.BagId, update.ItemId);

        state.Bags[update.BagId][update.ItemId] = (int) update.Value;
    }

    private void AddToBag(GameState definition, SaveState state, AddToAmountInBagUpdate update)
    {
        ValidateUpdateValid(definition, state, update.BagId, update.ItemId);

        var target = state.Bags[update.BagId][update.ItemId] + (int) update.Amount;
        ValidateUpdateInRange(update.BagId, target);

        state.Bags[update.BagId][update.ItemId] = target;
    }

    private void RemoveFromBag(GameState definition, SaveState state, SubtractFromAmountInBagUpdate update)
    {
        ValidateUpdateValid(definition, state, update.BagId, update.ItemId);

        var target = state.Bags[update.BagId][update.ItemId] - (int) update.Amount;
        ValidateUpdateInRange(update.BagId, target);

        state.Bags[update.BagId][update.ItemId] = target;
    }

    private void ClearBag(GameState definition, SaveState state, ClearFromBagUpdate update)
    {
        ValidateUpdateValid(definition, state, update.BagId, update.ItemId);

        state.Bags[update.BagId].Clear();
    }

    private void ValidateUpdateValid(GameState definition, SaveState state, string stateId, string bagItemId)
    {
        if (!definition.Bags.ContainsKey(stateId))
            throw new InvalidGameStateException($"Tried to read undefined bag {stateId}.");

        if (!state.Bags.ContainsKey(stateId))
            throw new GameWizardInternalException($"Could not find bag {stateId} in loaded state.");

        if (!state.Bags[stateId].ContainsKey(bagItemId))
            state.Bags[stateId][bagItemId] = 0;
    }

    private void ValidateUpdateInRange(string stateId, decimal target)
    {
        if (target < 0)
            throw new GameWizardInternalException();
    }
}