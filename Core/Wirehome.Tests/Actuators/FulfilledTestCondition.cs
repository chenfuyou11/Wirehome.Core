using Wirehome.Conditions;
using Wirehome.Contracts.Conditions;

namespace Wirehome.Tests.Actuators
{
    public class FulfilledTestCondition : Condition
    {
        public FulfilledTestCondition()
        {
            WithExpression(() => ConditionState.Fulfilled);
        }
    }
}
