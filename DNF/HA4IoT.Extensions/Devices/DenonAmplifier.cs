using HA4IoT.Components;
using System;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Components.States;
using HA4IoT.Contracts.Components.Features;
using HA4IoT.Contracts.Components.Commands;
using HA4IoT.Components.Commands;
using System.Xml;
using System.Net.Http;
using System.Threading.Tasks;
using HA4IoT.Contracts.Messaging;
using HA4IoT.Extensions.Messaging;
using HA4IoT.Extensions.Messaging.Services;
using HA4IoT.Extensions.Messaging.DenonMessages;

namespace HA4IoT.Extensions.Devices
{
    public class DenonAmplifier : ComponentBase
    {
        private PowerStateValue _powerState = PowerStateValue.Off;
        private CommandExecutor _commandExecutor;
        private readonly string _denonControlAddress;
        private readonly string _denonConfigAddress;
        private readonly IMessageBrokerService _messageBrokerService;

        public DenonAmplifier(string id, string hostname, IMessageBrokerService messageBroker) : base(id)
        {
            _commandExecutor = new CommandExecutor();
            _commandExecutor.Register<TurnOnCommand>(c => 
            {
                _messageBrokerService.Publish(typeof(HttpMessagingService).Name, new DenonControlMessage
                {
                    Zone = "1",
                    Command = "PowerOn",
                    Api = "formiPhoneAppPower",
                    ReturnNode = "Power",
                    Address = hostname
                });
                
            });
            _commandExecutor.Register<TurnOffCommand>(c =>
            {
                _messageBrokerService.Publish(typeof(HttpMessagingService).Name, new DenonControlMessage
                {
                    Zone = "1",
                    Command = "PowerOff",
                    Api = "formiPhoneAppPower",
                    ReturnNode = "Power",
                    Address = hostname
                });
            }
            );
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
