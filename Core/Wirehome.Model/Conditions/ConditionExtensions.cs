using System;
using Wirehome.Contracts.Conditions;

namespace Wirehome.Conditions
{
    public static class ConditionExtensions
    {
        public static bool IsFulfilled(this ICondition condition)
        {
            if (condition == null) throw new ArgumentNullException(nameof(condition));

            return condition.Validate() == ConditionState.Fulfilled;
        }
    }
}
