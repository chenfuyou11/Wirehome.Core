using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;
using Wirehome.ComponentModel.Adapters;
using Wirehome.ComponentModel.Adapters.Denon;
using Wirehome.ComponentModel.Commands;
using Wirehome.ComponentModel.Commands.Responses;
using Wirehome.ComponentModel.Extensions;
using Wirehome.Core.EventAggregator;
using Wirehome.Core.Tests.ComponentModel;
using Wirehome.Core.Tests.Mocks;

namespace Wirehome.Extensions.Tests
{
    [TestClass]
    public class AdaptersTests : ReactiveTest
    {
        [TestMethod]
        public async Task CreateAdapter()
        {
            var configuration = await CommonIntegrationcs.ReadConfiguration("componentConiguration");

            var adapter = configuration.config.Adapters.FirstOrDefault(x => x.Uid == "HSRel8_1");

            await adapter.Initialize();
        }

        [TestMethod]
        public async Task AdapterCommandExecuteShouldGetResult()
        {
            var container = CommonIntegrationcs.PrepareContainer();
            var adapterServiceFactory = container.GetInstance<IAdapterServiceFactory>();
            var adapter = new TestAdapter(adapterServiceFactory);
            await adapter.Initialize();

            var result = await adapter.ExecuteCommand(Command.DiscoverCapabilitiesCommand);

            Assert.IsInstanceOfType(result, typeof(DiscoveryResponse));
        }

        [TestMethod]
        public async Task AdapterCommandViaEventAggregatorExecuteShouldGetResult()
        {
            var container = CommonIntegrationcs.PrepareContainer();
            var adapterServiceFactory = container.GetInstance<IAdapterServiceFactory>();
            var eventAggregator = container.GetInstance<IEventAggregator>();
            var adapter = new TestAdapter(adapterServiceFactory);
            await adapter.Initialize();

            var adapterCapabilities = await eventAggregator.QueryDeviceAsync<DeviceCommand, DiscoveryResponse>(new DeviceCommand(CommandType.DiscoverCapabilities, adapter.Uid), TimeSpan.FromMilliseconds(4000));

            Assert.IsInstanceOfType(adapterCapabilities, typeof(DiscoveryResponse));
        }
    }
}