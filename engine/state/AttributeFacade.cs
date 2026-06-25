using GameWizard.Engine.Schema.Game;

namespace GameWizard.Engine.State;

public class AttributeFacade : StateFacade
{
    public AttributeFacade()
    {
        RegisterUpdater<SetAttributeUpdate>(SetAttribute);
        RegisterUpdater<AddToAttributeUpdate>(AddAttribute);
        RegisterUpdater<SubtractFromAttributeUpdate>(SubtractAttribute);
    }

    public decimal GetAttribute(GameState definition, SaveState state, string stateId)
    {
        ValidateUpdateValid(definition, state, stateId);

        return state.Attributes[stateId];
    }

    private void SetAttribute(GameState definition, SaveState state, SetAttributeUpdate update)
    {
        ValidateUpdateValid(definition, state, update.AttributeId);
        ValidateUpdateInRange(update.AttributeId, definition.Attributes[update.AttributeId], update.Value);

        state.Attributes[update.AttributeId] = update.Value;
    }

    private void AddAttribute(GameState definition, SaveState state, AddToAttributeUpdate update)
    {
        ValidateUpdateValid(definition, state, update.AttributeId);

        var target = state.Attributes[update.AttributeId] + update.Amount;
        ValidateUpdateInRange(update.AttributeId, definition.Attributes[update.AttributeId], target);

        state.Attributes[update.AttributeId] = target;
    }

    private void SubtractAttribute(GameState definition, SaveState state, SubtractFromAttributeUpdate update)
    {
        ValidateUpdateValid(definition, state, update.AttributeId);

        var target = state.Attributes[update.AttributeId] - update.Amount;
        ValidateUpdateInRange(update.AttributeId, definition.Attributes[update.AttributeId], target);

        state.Attributes[update.AttributeId] = target;
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