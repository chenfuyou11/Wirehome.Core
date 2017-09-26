using System;
using Wirehome.Contracts.Actuators;
using Wirehome.Contracts.Areas;
using Wirehome.Contracts.Components.Commands;
using Wirehome.Contracts.Core;

namespace Wirehome.Actuators.StateMachines
{
    public static class StateMachineExtensions
    {
        public static IStateMachine GetStateMachine(this IArea area, Enum id)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            return area.GetComponent<IStateMachine>($"{area.Id}.{id}");
        }

        public static bool GetSupportsOffState(this IStateMachine stateMachine)
        {
            if (stateMachine == null) throw new ArgumentNullException(nameof(stateMachine));

            return stateMachine.SupportsState(StateMachineStateExtensions.OffStateId);
        }

        public static bool GetSupportsOnState(this IStateMachine stateMachine)
        {
            if (stateMachine == null) throw new ArgumentNullException(nameof(stateMachine));

            return stateMachine.SupportsState(StateMachineStateExtensions.OnStateId);
        }

        public static StateMachineState AddOffState(this StateMachine stateMachine)
        {
            if (stateMachine == null) throw new ArgumentNullException(nameof(stateMachine));

            return stateMachine.AddState(StateMachineStateExtensions.OffStateId);
        }

        public static StateMachineState AddOnState(this StateMachine stateMachine)
        {
            if (stateMachine == null) throw new ArgumentNullException(nameof(stateMachine));

            return stateMachine.AddState(StateMachineStateExtensions.OnStateId);
        }

        public static StateMachineState AddState(this StateMachine stateMachine, string id)
        {
            if (stateMachine == null) throw new ArgumentNullException(nameof(stateMachine));
            if (id == null) throw new ArgumentNullException(nameof(id));

            var state = new StateMachineState(id);
            stateMachine.RegisterState(state);
            return state;
        }

        public static IAction GetSetStateAction(this IStateMachine stateMachine, string id)
        {
            if (stateMachine == null) throw new ArgumentNullException(nameof(stateMachine));
            if (id == null) throw new ArgumentNullException(nameof(id));

            return new ActionWrapper(() => stateMachine.ExecuteCommand(new SetStateCommand { Id = id }));
        }
    }
}
