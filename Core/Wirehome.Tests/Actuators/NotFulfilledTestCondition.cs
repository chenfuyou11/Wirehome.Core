using Wirehome.Conditions;
using Wirehome.Contracts.Conditions;

namespace Wirehome.Tests.Actuators
{
    public class NotFulfilledTestCondition : Condition
    {
        public NotFulfilledTestCondition()
        {
            WithExpression(() => ConditionState.NotFulfilled);
        }
    }
}
