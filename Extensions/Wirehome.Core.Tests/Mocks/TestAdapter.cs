using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Wirehome.ComponentModel.Adapters;
using Wirehome.ComponentModel.Capabilities;
using Wirehome.ComponentModel.Commands;
using Wirehome.ComponentModel.Commands.Responses;
using Wirehome.Core.EventAggregator;
using Wirehome.ComponentModel.Extensions;
using Wirehome.Core.Extensions;

namespace Wirehome.Core.Tests.Mocks
{
    public class TestAdapter : Adapter
    {
        public TestAdapter(IAdapterServiceFactory adapterServiceFactory) : base(adapterServiceFactory)
        {
            Uid = "adapter_1";
        }

        public override async Task Initialize()
        {
            _disposables.Add(_eventAggregator.SubscribeForDeviceQuery<DeviceCommand>(DeviceCommandHandler, Uid));

            base.Initialize();
        }

        private Task<object> DeviceCommandHandler(IMessageEnvelope<DeviceCommand> messageEnvelope)
        {
            return ExecuteCommand(messageEnvelope.Message, messageEnvelope.CancellationToken);
        }

        protected async Task<object> DiscoverCapabilitiesHandler(Command message)
        {
            await Task.Delay(2000);

            return new DiscoveryResponse(RequierdProperties(), new PowerState());
        }
    }
}