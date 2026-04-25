using System.Collections.Generic;
using System.Linq;
using GameWizard.Engine.Schema;
using GameWizard.Engine.State;
using Godot;

namespace GameWizard.Engine.Util;

public static class GWConditionEvaluator
{
    public static bool Evaluate(GWStateCache state, IList<GWCondition> conditions)
    {
        if (conditions is null or []) return true;

        return conditions.All(condition =>
        {
            switch (condition.Type)
            {
                case GWConditionType.Flag:
                    var value = state.Read(condition.Name);
                    return CompareState(condition.Targets, condition.Comparator, value);
                default:
                    GD.PushError($"Received unexpected state type {condition.Type}.");
                    return false;
            }
        });
    }

    private static bool CompareState(IList<string> expected, GWConditionComparator comparator, string actual)
    {
        switch (comparator)
        {
            case GWConditionComparator.OneOf:
                return expected.Contains(actual);
            default:
                GD.PushError($"Received unexpected condition comparator {comparator}.");
                return false;
        }
    }
}