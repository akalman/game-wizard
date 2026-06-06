using System.Collections.Generic;
using System.Linq;
using GameWizard.Engine.Schema.Logic;
using GameWizard.Engine.State;
using Godot;

namespace GameWizard.Engine.Util;

public static class GameWizardExtensions
{
    public static bool Evaluate(this IList<Condition> conditions, IStateRepository state)
    {
        if (conditions is null or []) return true;

        return conditions.All(condition =>
        {
            return condition.Type switch
            {
                ConditionType.FlagIn => condition.ExpectedMembership.Contains(state.ReadFlag(condition.Target)),
                ConditionType.AttributeLessThan => state.ReadAttribute(condition.Target) < condition.ExpectedNumber,
                ConditionType.AttributeMoreThan => state.ReadAttribute(condition.Target) > condition.ExpectedNumber,
                ConditionType.BagContainsMoreThan => state.NumInBag(condition.BagTarget, condition.Target) > condition.ExpectedNumber,
                _ => throw new GameWizardInternalException($"Encountered unexpected condition type {condition.Type}."),
            };
        });
    }
}