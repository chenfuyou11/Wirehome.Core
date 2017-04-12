using System;
using HA4IoT.Commands;
using HA4IoT.Components;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Adapters;
using HA4IoT.Contracts.Commands;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Components.Features;
using HA4IoT.Contracts.Components.States;
using HA4IoT.Contracts.Hardware;
using System.Collections.Generic;

namespace HA4IoT.Extensions.Core
{
    public class MonostableLamp : ComponentBase, ILamp
    {
        private readonly IMonostableLampAdapter _adapter;

        private PowerStateValue _powerState = PowerStateValue.Off;
        private CommandExecutor _commandExecutor;

        public MonostableLamp(string id, IMonostableLampAdapter adapter)
            : base(id)
        {
            _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));

            adapter.StateChanged += Adapter_StateChanged;

            _commandExecutor = new CommandExecutor();
            _commandExecutor.Register<ResetCommand>(c => ResetState());
            _commandExecutor.Register<TurnOnCommand>(c => SetStateInternal(PowerStateValue.On));
            _commandExecutor.Register<TurnOffCommand>(c => SetStateInternal(PowerStateValue.Off));
            _commandExecutor.Register<TogglePowerStateCommand>(c => TogglePowerState());
        }

        private void Adapter_StateChanged(PowerStateValue value)
        {
            _powerState = value;
        }

        public override IComponentFeatureStateCollection GetState()
        {
            var state = new ComponentFeatureStateCollection()
                .With(new PowerState(_powerState));

            return state;
        }

      
        public override IComponentFeatureCollection GetFeatures()
        {
            var features = new ComponentFeatureCollection()
                .With(new PowerStateFeature());

            return features;
        }

        public override void ExecuteCommand(ICommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            
            _commandExecutor.Execute(command);
        }

        public void ResetState()
        {
            SetStateInternal(PowerStateValue.Off, true);
        }

        private void TogglePowerState()
        {
            SetStateInternal(_powerState == PowerStateValue.Off ? PowerStateValue.On : PowerStateValue.Off);
        }

        private void SetStateInternal(PowerStateValue powerState, bool forceUpdate = false)
        {
            
            if (!forceUpdate && _powerState == powerState)
            {
                return;
            }

            var oldState = GetState();

            var parameters = forceUpdate ? new IHardwareParameter[] { HardwareParameter.ForceUpdateState } : new IHardwareParameter[0];
            if (powerState == PowerStateValue.On)
            {
                _adapter.SetState(AdapterPowerState.On, null, parameters);
            }
            else if (powerState == PowerStateValue.Off)
            {
                _adapter.SetState(AdapterPowerState.Off, null, parameters);
            }

            _powerState = powerState;

            OnStateChanged(oldState);
        }
    }
}
