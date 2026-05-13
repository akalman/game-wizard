using System.Collections.Generic;
using System.Linq;
using GameWizard.Engine.Schema.Logic;
using GameWizard.Engine.State;

namespace GameWizard.Engine.Util;

public static class GameWizardExtensions
{
    public static bool Evaluate(this IList<Condition> conditions, IStateRepository state)
    {
        if (conditions is null or []) return true;

        return conditions.All(condition =>
        {
            switch (condition.Type)
            {
                case ConditionType.FlagIn:
                    var value = state.ReadFlag(condition.Target);
                    return condition.ExpectedMembership.Contains(value);
                default:
                    throw new GameWizardInternalException($"Encountered unexpected condition type {condition.Type}.");
            }
        });
    }
}