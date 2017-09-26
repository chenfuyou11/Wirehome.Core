using System;
using Wirehome.Components;
using Wirehome.Components.Commands;
using Wirehome.Contracts.Actuators;
using Wirehome.Contracts.Components;
using Wirehome.Contracts.Components.Adapters;
using Wirehome.Contracts.Components.Commands;
using Wirehome.Contracts.Components.Features;
using Wirehome.Contracts.Components.States;
using Wirehome.Contracts.Hardware;

namespace Wirehome.Actuators.Sockets
{
    public class Socket : ComponentBase, ISocket
    {
        private readonly object _syncRoot = new object();

        private readonly CommandExecutor _commandExecutor = new CommandExecutor();
        private readonly IBinaryOutputAdapter _adapter;

        private PowerStateValue _powerState = PowerStateValue.Off;

        public Socket(string id, IBinaryOutputAdapter adapter) : base(id)
        {
            _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));

            _commandExecutor.Register<TurnOnCommand>(c => SetStateInternal(PowerStateValue.On));
            _commandExecutor.Register<TurnOffCommand>(c => SetStateInternal(PowerStateValue.Off));
            _commandExecutor.Register<TogglePowerStateCommand>(c => TogglePowerState());
            _commandExecutor.Register<ResetCommand>(c => SetStateInternal(PowerStateValue.Off, true));
        }

        public override IComponentFeatureStateCollection GetState()
        {
            return new ComponentFeatureStateCollection()
                .With(new PowerState(_powerState));
        }

        public override IComponentFeatureCollection GetFeatures()
        {
            return new ComponentFeatureCollection()
                .With(new PowerStateFeature());
        }

        public override void ExecuteCommand(ICommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            lock (_syncRoot)
            {
                _commandExecutor.Execute(command);
            }
        }

        public void ResetState()
        {
            lock (_syncRoot)
            {
                SetStateInternal(PowerStateValue.Off, true);
            }
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
                _adapter.SetState(AdapterPowerState.On, parameters);
            }
            else if (powerState == PowerStateValue.Off)
            {
                _adapter.SetState(AdapterPowerState.Off, parameters);
            }

            _powerState = powerState;

            OnStateChanged(oldState);
        }
    }
}