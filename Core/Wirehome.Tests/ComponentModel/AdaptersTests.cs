﻿using Microsoft.Reactive.Testing;
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
            await adapter.Initialize();

            var result = await adapter.ExecuteCommand<DiscoveryResponse>(CommandFatory.DiscoverCapabilitiesCommand);

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
            await adapter.Initialize();

            var taskList = new List<Task>();

            for (int i = 0; i < 10; i++)
            {
                taskList.Add(Task.Run(() => adapter.ExecuteCommand(CommandFatory.RefreshCommand)));
            }

            await Task.WhenAll(taskList);

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
            await adapter.Initialize();

            var result = await eventAggregator.QueryDeviceAsync<DiscoveryResponse>(DeviceCommand.GenerateDiscoverCommand(adapter.Uid));

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
            await adapter.Initialize();

            var result = await eventAggregator.QueryDeviceAsync<DiscoveryResponse>(DeviceCommand.GenerateDiscoverCommand(adapter.Uid), TimeSpan.FromMilliseconds(20));
        }

        [TestMethod]
        public async Task GenerateAdapter()
        {
            var (controller, container) = await new ControllerBuilder().WithConfiguration("oneComponentConfiguration")
                                                                       .BuildAndRun()
                                                                       .ConfigureAwait(false);
            var adapterServiceFactory = container.GetInstance<IAdapterServiceFactory>();
            var roslynGenerator = new RoslynAsseblyGenerator();
            var assembly = roslynGenerator.GenerateAssembly("adapter.dll", GetAdapterDir(), AssemblyHelper.GetReferencedAssemblies(typeof(Adapter)));

            if (assembly.IsSuccess)
            {
                Assembly asm = AssemblyLoadContext.Default.LoadFromAssemblyPath(assembly.Value);
                var adapterType = asm.GetType("Wirehome.ComponentModel.Adapters.Kodi.KodiAdapterTest");
                var adapter = Activator.CreateInstance(adapterType, new Object[] { adapterServiceFactory }) as Adapter;

                await adapter.Initialize();
                var result = await adapter.ExecuteCommand("TestCommand");
            }
        }

        private string GetAdapterDir()
        {
            var dir = Directory.GetCurrentDirectory();
            return Path.Combine(dir.Substring(0, dir.IndexOf("Wirehome.Core.Tests")), @"Adapters\TestAdapter\Kodi");
        }
    }
}