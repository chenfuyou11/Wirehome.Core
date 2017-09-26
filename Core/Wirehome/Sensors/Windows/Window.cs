using System;
using Wirehome.Components;
using Wirehome.Components.Commands;
using Wirehome.Contracts.Components;
using Wirehome.Contracts.Components.Adapters;
using Wirehome.Contracts.Components.Commands;
using Wirehome.Contracts.Components.Features;
using Wirehome.Contracts.Components.States;
using Wirehome.Contracts.Messaging;
using Wirehome.Contracts.Sensors;
using Wirehome.Contracts.Sensors.Events;
using Wirehome.Contracts.Settings;

namespace Wirehome.Sensors.Windows
{
    public class Window : ComponentBase, IWindow
    {
        private readonly IMessageBrokerService _messageBroker;
        private readonly IWindowAdapter _adapter;
        private readonly ISettingsService _settingsService;
        private WindowStateValue _state;

        public Window(string id, IWindowAdapter adapter, ISettingsService settingsService, IMessageBrokerService messageBroker)
            : base(id)
        {
            _messageBroker = messageBroker ?? throw new ArgumentNullException(nameof(messageBroker));
            _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));

            adapter.StateChanged += (s, e) => Update(e);
        }

        public override IComponentFeatureCollection GetFeatures()
        {
            return new ComponentFeatureCollection()
                .With(new WindowStateFeature());
        }

        public override IComponentFeatureStateCollection GetState()
        {
            return new ComponentFeatureStateCollection()
                .With(new WindowState(_state));
        }

        public override void ExecuteCommand(ICommand command)
        {
            var commandExecutor = new CommandExecutor();
            commandExecutor.Register<ResetCommand>(c => _adapter.Refresh());
            commandExecutor.Execute(command);
        }
        
        private void Update(WindowStateChangedEventArgs eventArgs)
        {
            WindowStateValue newState;
            if (eventArgs.OpenReedSwitchState == AdapterSwitchState.Open)
            {
                newState = WindowStateValue.Open;
            }
            else if (eventArgs.TildReedSwitchState.HasValue && eventArgs.TildReedSwitchState.Value == AdapterSwitchState.Open)
            {
                newState = WindowStateValue.TildOpen;
            }
            else
            {
                newState = WindowStateValue.Closed;
            }

            if (newState.Equals(_state))
            {
                return;
            }

            var oldState = GetState();
            _state = newState;
            
            if (!_settingsService.GetSettings<ComponentSettings>(this).IsEnabled)
            {
                return;
            }

            OnStateChanged(oldState);

            if (_state == WindowStateValue.Closed)
            {
                _messageBroker.Publish(Id, new WindowClosedEvent());
            }
            else
            {
                _messageBroker.Publish(Id, new WindowOpenedEvent());
            }
        }
    }
}