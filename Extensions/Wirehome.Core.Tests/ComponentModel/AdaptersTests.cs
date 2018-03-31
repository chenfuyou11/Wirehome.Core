using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;
using Wirehome.ComponentModel.Adapters;
using Wirehome.ComponentModel.Commands;
using Wirehome.ComponentModel.Commands.Responses;
using Wirehome.ComponentModel.Extensions;
using Wirehome.Core.EventAggregator;
using Wirehome.Core.Tests.ComponentModel;
using Wirehome.Core.Tests.Mocks;
using Wirehome.Core.Extensions;
using Wirehome.ComponentModel.Capabilities;
using System.Collections.Generic;

namespace Wirehome.Extensions.Tests
{
    [TestClass]
    public class AdaptersTests : ReactiveTest
    {
        [TestMethod]
        public async Task AdapterCommandExecuteShouldGetResult()
        {
            var container = CommonIntegrationcs.PrepareContainer();
            var adapterServiceFactory = container.GetInstance<IAdapterServiceFactory>();
            var adapter = new TestAdapter("adapter1", adapterServiceFactory);
            await adapter.Initialize();

            var result = await adapter.ExecuteCommand(Command.DiscoverCapabilitiesCommand).Cast<DiscoveryResponse>();

            Assert.AreEqual(1, result.SupportedStates.Length);
            Assert.IsInstanceOfType(result.SupportedStates[0], typeof(PowerState));
        }

        [TestMethod]
        public async Task MultiThreadAdapterCommandsExecuteShouldBeQueued()
        {
            var container = CommonIntegrationcs.PrepareContainer();
            var adapterServiceFactory = container.GetInstance<IAdapterServiceFactory>();
            var adapter = new TestAdapter("adapter1", adapterServiceFactory);
            await adapter.Initialize();

            var taskList = new List<Task>();

            for (int i = 0; i < 10; i++)
            {
                taskList.Add(Task.Run(() => adapter.ExecuteCommand(Command.RefreshCommand)));
            }

            await Task.WhenAll(taskList);

            Assert.AreEqual(0, adapter.Counter);
        }

        [TestMethod]
        public async Task AdapterCommandViaEventAggregatorExecuteShouldGetResult()
        {
            var container = CommonIntegrationcs.PrepareContainer();
            var adapterServiceFactory = container.GetInstance<IAdapterServiceFactory>();
            var eventAggregator = container.GetInstance<IEventAggregator>();
            var adapter = new TestAdapter("adapter1", adapterServiceFactory);
            await adapter.Initialize();

            var result = await eventAggregator.QueryDeviceAsync<DiscoveryResponse>(DeviceCommand.GenerateDiscoverCommand(adapter.Uid));

            Assert.AreEqual(1, result.SupportedStates.Length);
            Assert.IsInstanceOfType(result.SupportedStates[0], typeof(PowerState));
        }

        [TestMethod]
        [ExpectedException(typeof(TimeoutException))]
        public async Task AdapterCommandViaEventAggregatorExecuteShouldTimeoutWhenExecuteToLong()
        {
            var container = CommonIntegrationcs.PrepareContainer();
            var adapterServiceFactory = container.GetInstance<IAdapterServiceFactory>();
            var eventAggregator = container.GetInstance<IEventAggregator>();
            var adapter = new TestAdapter("adapter1", adapterServiceFactory);
            await adapter.Initialize();

            var result = await eventAggregator.QueryDeviceAsync<DiscoveryResponse>(DeviceCommand.GenerateDiscoverCommand(adapter.Uid), TimeSpan.FromMilliseconds(100));
        }
    }
}