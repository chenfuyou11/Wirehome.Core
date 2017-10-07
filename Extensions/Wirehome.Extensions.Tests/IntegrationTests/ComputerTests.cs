using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Quartz;
using System;
using System.Threading.Tasks;
using Wirehome.Contracts.Components.Commands;
using Wirehome.Contracts.Core;
using Wirehome.Contracts.Logging;
using Wirehome.Core;
using Wirehome.Extensions.Devices;
using Wirehome.Extensions.Devices.Commands;
using Wirehome.Extensions.Devices.Sony;
using Wirehome.Extensions.Messaging.Core;
using Wirehome.Extensions.Messaging.Services;
using Wirehome.Extensions.Quartz;

namespace Wirehome.Extensions.Tests.IntegrationTests
{
    [TestClass]
    [TestCategory("Integration")]
    public class ComputerTests
    {
        public const string HOST = "localhost";


        private (IEventAggregator ev, IScheduler ch) PrepareMocks()
        {
            var log = Mock.Of<ILogService>();

            var container = new Container(new ControllerOptions());
            container.RegisterSingleton<IContainer>(() => container);
            container.RegisterSingleton<IEventAggregator, EventAggregator>();
            container.RegisterSingleton(log);
            container.RegisterQuartz();

            var scheduler = container.GetInstance<IScheduler>();
            var eventAggregator = container.GetInstance<IEventAggregator>();

            var http = new HttpMessagingService(eventAggregator);
            http.Initialize();

            return (eventAggregator, scheduler);
        }

        [TestMethod]
        public async Task ComputerVolumeTest()
        {
            var mocks = PrepareMocks();

            var computer = new ComputerDevice(Guid.NewGuid().ToString(), mocks.ev, mocks.ch)
            {
                Hostname = HOST,
                Port = 5000
            };
            await computer.Initialize();

            await Task.Delay(5000);
            //await computer.ExecuteAsyncCommand(new SetVolumeCommand { Volume = 60 }).ConfigureAwait(false);
        }
        //
    }
}
