using System;
using Wirehome.Contracts.Conditions;
using Wirehome.Contracts.Triggers;

namespace Wirehome.Triggers
{
    public class ConditionalTrigger : Trigger
    {
        private ICondition _condition;

        public ConditionalTrigger WithTrigger(ITrigger trigger)
        {
            if (trigger == null) throw new ArgumentNullException(nameof(trigger));

            trigger.Attach(ForwardTriggerEvent);
            return this;
        }

        public ConditionalTrigger WithCondition(ICondition condition)
        {
            _condition = condition;
            return this;
        }

        private void ForwardTriggerEvent()
        {
            if (_condition != null)
            {
                if (_condition.Validate() != ConditionState.Fulfilled)
                {
                    return;
                }
            }

            Execute();
        }
    }
}
