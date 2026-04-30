using System.Collections.Generic;
using System.Linq;
using GameWizard.Engine.Schema;
using Godot;

namespace GameWizard.Engine.State;

public static class GWConditionEvaluator
{
    public static bool Evaluate(GWStateFacade state, IList<GWCondition> conditions)
    {
        if (conditions is null or []) return true;

        return conditions.All(condition =>
        {
            switch (condition.Predicate)
            {
                case GWStatePredicate.FlagIn:
                    var value = state.Read(condition.Target);
                    return condition.Set.Contains(value);
                default:
                    GD.PushError($"Received unexpected state type {condition.Predicate}.");
                    return false;
            }
        });
    }
}