using HA4IoT.Components;
using System;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Components.States;
using HA4IoT.Contracts.Components.Features;
using HA4IoT.Contracts.Components.Commands;
using HA4IoT.Components.Commands;
using HA4IoT.Extensions.Messaging.DenonMessages;
using HA4IoT.Extensions.Messaging.Core;

namespace HA4IoT.Extensions.Devices
{
    public class DenonAmplifier : ComponentBase
    {
        private readonly CommandExecutor _commandExecutor;
        private readonly IEventAggregator _eventAggregator;

        private PowerStateValue _powerState = PowerStateValue.Off;

        public string Hostname { get; }

        public DenonAmplifier(string id, string hostname, IEventAggregator eventAggregator) : base(id)
        {
            _commandExecutor = new CommandExecutor();
            Hostname = hostname ?? throw new ArgumentNullException(nameof(hostname));
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));

            //_commandExecutor.Register<TurnOnCommand>(async c => 
            //{
            //    var result = await _eventAggregator.PublishWithResultAsync<DenonControlMessage, string>(new DenonControlMessage
            //    {
            //        Command = "PowerOn",
            //        Api = "formiPhoneAppPower",
            //        ReturnNode = "Power",
            //        Address = Hostname
            //    });
                
            //});
            //_commandExecutor.Register<TurnOffCommand>(c =>
            //{
            //    _eventAggregator.PublishAsync(new DenonControlMessage
            //    {
            //        Command = "PowerOff",
            //        Api = "formiPhoneAppPower",
            //        ReturnNode = "Power",
            //        Address = Hostname
            //    });
            //}
            //);            
        }

        public override void ExecuteCommand(ICommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            _commandExecutor.Execute(command);
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

        private void SetStateInternal(PowerStateValue powerState, bool forceUpdate = false)
        {

            if (!forceUpdate && _powerState == powerState)
            {
                return;
            }
            
            var oldState = GetState();
            
            _powerState = powerState;

            OnStateChanged(oldState);
        }

  
    }

    
}
