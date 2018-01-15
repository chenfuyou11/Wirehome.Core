using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Collections.Generic;
using Wirehome.Conditions;
using Wirehome.Conditions.Specialized;
using Wirehome.Contracts.Conditions;
using Wirehome.Contracts.Components.Commands;
using Wirehome.Contracts.Components.States;
using Wirehome.Contracts.Environment;
using Wirehome.Contracts.Core;
using Wirehome.Contracts.Components;
using System.Collections.ObjectModel;

namespace Wirehome.Motion.Model
{

    public class IsEnabledAutomationCondition : Condition
    {
        private readonly MotionDescriptor _motionDescriptor;

        public IsEnabledAutomationCondition(MotionDescriptor motionDescriptor)
        {
            _motionDescriptor = motionDescriptor;

            WithExpression(() => _motionDescriptor.AutomationDisabled ? ConditionState.NotFulfilled : ConditionState.Fulfilled);
        }
    }

}