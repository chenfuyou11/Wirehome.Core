using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;
using Wirehome.Contracts.Components.Commands;
using Wirehome.Contracts.Core;
using Wirehome.Contracts.Logging;
using Wirehome.Core;
using Wirehome.Extensions.Devices.Samsung;
using Wirehome.Extensions.Messaging.Core;
using Wirehome.Extensions.Messaging.Services;

namespace Wirehome.Extensions.Tests.IntegrationTests
{
    [TestClass]
    [TestCategory("Integration")]
    public class SamsungTVTests
    {
        public const string SAMSUNG_HOST = "192.168.0.106";

        private IEventAggregator PrepareMocks()
        {
            var log = Mock.Of<ILogService>();

            var container = new Container(new ControllerOptions());
            container.RegisterSingleton<IContainer>(() => container);
            container.RegisterSingleton<IEventAggregator, EventAggregator>();
            container.RegisterSingleton(log);

            var eventAggregator = container.GetInstance<IEventAggregator>();

            var tcp = new TcpMessagingService(log, eventAggregator);
            tcp.Initialize();

            return eventAggregator;
        }

        [TestMethod]
        public async Task SamsungPowerTest()
        {
            var ev = PrepareMocks();

            var samsung = new SamsungTV(Guid.NewGuid().ToString(), ev)
            {
                Hostname = SAMSUNG_HOST
            };
            await samsung.Initialize();

            await samsung.ExecuteAsyncCommand<TurnOffCommand>().ConfigureAwait(false);
        }


        //https://github.com/openremote/Documentation/wiki/Samsung-Smart-TV
        
    }
}
