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
using Wirehome.Core.Services.Roslyn;
using System.Reflection;
using System.Globalization;
using Wirehome.Core.Utils;
using Newtonsoft.Json;
using System.Runtime.Loader;
using System.IO;
using Wirehome.Model.Core;

namespace Wirehome.Extensions.Tests
{
    [TestClass]
    public class AdaptersTests : ReactiveTest
    {
        [TestMethod]
        public async Task AdapterCommandExecuteShouldGetResult()
        {
            var (controller, container) = await new ControllerBuilder().WithConfiguration("oneComponentConfiguration")
                                                                       .BuildAndRun()
                                                                       .ConfigureAwait(false);

            var adapterServiceFactory = container.GetInstance<IAdapterServiceFactory>();
            var adapter = new TestAdapter("adapter1", adapterServiceFactory);
            await adapter.Initialize().ConfigureAwait(false);

            var result = await adapter.ExecuteCommand<DiscoveryResponse>(CommandFatory.DiscoverCapabilitiesCommand).ConfigureAwait(false);

            Assert.AreEqual(1, result.SupportedStates.Length);
            Assert.IsInstanceOfType(result.SupportedStates[0], typeof(PowerState));
        }

        [TestMethod]
        public async Task MultiThreadAdapterCommandsExecuteShouldBeQueued()
        {
            var (controller, container) = await new ControllerBuilder().WithConfiguration("oneComponentConfiguration")
                                                                       .BuildAndRun()
                                                                       .ConfigureAwait(false);
            var adapterServiceFactory = container.GetInstance<IAdapterServiceFactory>();
            var adapter = new TestAdapter("adapter1", adapterServiceFactory);
            await adapter.Initialize().ConfigureAwait(false);

            var taskList = new List<Task>();

            for (int i = 0; i < 10; i++)
            {
                taskList.Add(Task.Run(() => adapter.ExecuteCommand(CommandFatory.RefreshCommand)));
            }

            await Task.WhenAll(taskList).ConfigureAwait(false);

            Assert.AreEqual(0, adapter.Counter);
        }

        [TestMethod]
        public async Task AdapterCommandViaEventAggregatorExecuteShouldGetResult()
        {
            var (controller, container) = await new ControllerBuilder().WithConfiguration("oneComponentConfiguration")
                                                                       .BuildAndRun()
                                                                       .ConfigureAwait(false);
            var adapterServiceFactory = container.GetInstance<IAdapterServiceFactory>();
            var eventAggregator = container.GetInstance<IEventAggregator>();
            var adapter = new TestAdapter("adapter1", adapterServiceFactory);
            await adapter.Initialize().ConfigureAwait(false);

            var result = await eventAggregator.QueryDeviceAsync<DiscoveryResponse>(DeviceCommand.GenerateDiscoverCommand(adapter.Uid)).ConfigureAwait(false);

            Assert.AreEqual(1, result.SupportedStates.Length);
            Assert.IsInstanceOfType(result.SupportedStates[0], typeof(PowerState));
        }

        [TestMethod]
        [ExpectedException(typeof(TimeoutException))]
        public async Task AdapterCommandViaEventAggregatorExecuteShouldTimeoutWhenExecuteToLong()
        {
            var (controller, container) = await new ControllerBuilder().WithConfiguration("oneComponentConfiguration")
                                                                       .BuildAndRun()
                                                                       .ConfigureAwait(false);
            var adapterServiceFactory = container.GetInstance<IAdapterServiceFactory>();
            var eventAggregator = container.GetInstance<IEventAggregator>();
            var adapter = new TestAdapter("adapter1", adapterServiceFactory);
            await adapter.Initialize().ConfigureAwait(false);

            var result = await eventAggregator.QueryDeviceAsync<DiscoveryResponse>(DeviceCommand.GenerateDiscoverCommand(adapter.Uid), TimeSpan.FromMilliseconds(20)).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task GenerateAdapter()
        {
            var (controller, container) = await new ControllerBuilder().WithConfiguration("oneComponentConfiguration")
                                                                       .BuildAndRun()
                                                                       .ConfigureAwait(false);
            var adapterServiceFactory = container.GetInstance<IAdapterServiceFactory>();
            var roslynGenerator = new RoslynAsseblyGenerator();

            var modelAssemblies = AssemblyHelper.GetReferencedAssemblies(typeof(Adapter));
            var servicesAssemblies = AssemblyHelper.GetReferencedAssemblies(typeof(WirehomeController));

            var referencedAssemblies = modelAssemblies.Union(servicesAssemblies).Distinct();

            var assembly = roslynGenerator.GenerateAssembly("adapter.dll", GetAdapterDir(), referencedAssemblies) ;

            if (assembly.IsSuccess)
            {
                Assembly asm = AssemblyLoadContext.Default.LoadFromAssemblyPath(assembly.Value);
                var adapterType = asm.GetType("Wirehome.ComponentModel.Adapters.Kodi.KodiAdapter");
                var adapter = Activator.CreateInstance(adapterType, new Object[] { adapterServiceFactory }) as Adapter;

                await adapter.Initialize().ConfigureAwait(false);
                var result = await adapter.ExecuteCommand("TestCommand").ConfigureAwait(false);
            }
        }

        private string GetAdapterDir()
        {
            return Path.Combine(Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\..\..")), @"Adapters\AdaptersContainer\Adapters\Kodi");
        }
    }
}