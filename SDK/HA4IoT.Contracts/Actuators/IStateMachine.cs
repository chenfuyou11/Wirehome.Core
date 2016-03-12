﻿using HA4IoT.Contracts.Actions;

namespace HA4IoT.Contracts.Actuators
{
    public interface IStateMachine : IActuator, IActuatorWithOffState
    {
        bool HasOffState { get; }

        void SetState(string id, params IHardwareParameter[] parameters);

        void SetNextState(params IHardwareParameter[] parameters);

        IHomeAutomationAction GetSetNextStateAction();

        IHomeAutomationAction GetSetStateAction(string state);
    }
}
