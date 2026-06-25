using System.Collections.Generic;
using System.Linq;
using GameWizard.Engine.Schema.Logic;
using GameWizard.Engine.State;
using Godot;

namespace GameWizard.Engine.Util;

public static class GameWizardExtensions
{
    public static bool Evaluate(this IList<ICondition> conditions, IStateRepository state)
    {
        if (conditions is null or []) return true;

        return conditions.All(condition =>
        {
            return condition switch
            {
                FlagInCondition c => c.AllowedValues.Contains(state.ReadFlag(c.FlagId)),
                AttributeEqualsCondition c => state.ReadAttribute(c.AttributeId) == c.ExpectedValue,
                AttributeMoreThanCondition c => state.ReadAttribute(c.AttributeId) > c.Threshold,
                AttributeLessThanCondition c => state.ReadAttribute(c.AttributeId) < c.Threshold,
                BagContainsMoreThanCondition c => state.NumInBag(c.BagId, c.ItemId) > c.Threshold,
                BagContainsLessThanCondition c => state.NumInBag(c.BagId, c.ItemId) < c.Threshold,

                _ => throw new GameWizardInternalException($"Encountered unexpected condition type {condition.GetType().Name}."),
            };
        });
    }
}