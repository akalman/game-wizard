using System.Collections.Generic;
using GameWizard.Engine.Schema.Game;

namespace GameWizard.Engine.State;

public class AttributeFacade : StateFacade
{
    protected override IDictionary<StateUpdateType, StateFacadeUpdateFunc> GetUpdaters()
    {
        return new Dictionary<StateUpdateType, StateFacadeUpdateFunc>
        {
            { StateUpdateType.SetAttribute, SetAttribute },
            { StateUpdateType.AddAttribute, AddAttribute },
            { StateUpdateType.SubtractAttribute, SubtractAttribute },
        };
    }

    public decimal GetAttribute(GameState definition, SaveState state, string stateId)
    {
        ValidateUpdateValid(definition, state, stateId);

        return state.Attributes[stateId];
    }

    private void SetAttribute(GameState definition, SaveState state, StateUpdate update)
    {
        ValidateUpdateValid(definition, state, update.StateName);
        ValidateUpdateInRange(update.StateName, definition.Attributes[update.StateName], update.Number);

        state.Attributes[update.StateName] = update.Number;
    }

    private void AddAttribute(GameState definition, SaveState state, StateUpdate update)
    {
        ValidateUpdateValid(definition, state, update.StateName);

        var target = state.Attributes[update.StateName] + update.Number;
        ValidateUpdateInRange(update.StateName, definition.Attributes[update.StateName], target);

        state.Attributes[update.StateName] = target;
    }

    private void SubtractAttribute(GameState definition, SaveState state, StateUpdate update)
    {
        ValidateUpdateValid(definition, state, update.StateName);

        var target = state.Attributes[update.StateName] - update.Number;
        ValidateUpdateInRange(update.StateName, definition.Attributes[update.StateName], target);

        state.Attributes[update.StateName] = target;
    }

    private void ValidateUpdateValid(GameState definition, SaveState state, string stateId)
    {
        if (!definition.Attributes.ContainsKey(stateId))
            throw new InvalidGameStateException($"Tried to read undefined attribute {stateId}.");

        if (!state.Attributes.ContainsKey(stateId))
            throw new GameWizardInternalException($"Could not find attribute {stateId} in loaded state.");
    }

    private void ValidateUpdateInRange(string stateId, StateAttribute attribute, decimal target)
    {
        if (attribute.Max < target)
            throw new InvalidGameStateException($"Tried to set attribute {stateId} to out of bounds number: {target}");

        if (attribute.Min > target)
            throw new InvalidGameStateException($"Tried to set attribute {stateId} to out of bounds number: {target}");
    }
}