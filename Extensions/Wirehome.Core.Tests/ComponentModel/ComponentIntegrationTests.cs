using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Quartz;
using Quartz.Spi;
using SimpleInjector;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Wirehome.ComponentModel.Adapters;
using Wirehome.ComponentModel.Commands;
using Wirehome.ComponentModel.Configuration;
using Wirehome.Core.Communication.I2C;
using Wirehome.Core.EventAggregator;
using Wirehome.Core.Services.DependencyInjection;
using Wirehome.Core.Services.Logging;
using Wirehome.Core.Services.Quartz;
using Wirehome.ComponentModel.Extensions;
using Wirehome.ComponentModel.Events;
using Wirehome.ComponentModel.Capabilities;
using Wirehome.ComponentModel.ValueTypes;
using System.Collections.Generic;
using Wirehome.Core.Tests.ComponentModel;

namespace Wirehome.Extensions.Tests
{
    [TestClass]
    public class ComponentIntegrationTests : ReactiveTest
    {
        [TestMethod]
        public async Task TestReadComponentsFromConfig()
        {
            var config = await CommonIntegrationcs.ReadConfiguration("componentConiguration");
            var eventAggregator = config.container.GetInstance<IEventAggregator>();

            var properyChangeEvent = new PropertyChangedEvent("HSPE16InputOnly_1", PowerState.StateName, new BooleanValue(false),
                                     new BooleanValue(true), new Dictionary<string, IValue>() { { AdapterProperties.PinNumber, new IntValue(2) } });

            await eventAggregator.PublishDeviceEvent(properyChangeEvent, new string[] { AdapterProperties.PinNumber });

            //var lamp = configuration.Components.FirstOrDefault(c => c.Uid == "Lamp1");
            //await lamp.ExecuteCommand(new Command { Type = CommandType.TurnOn }).ConfigureAwait(false);

            //await Task.Delay(5000);
        }

        public void Test(KeyValuePair<string, IValue> value)
        {
        }

        [TestMethod]
        public async Task TestComponent()
        {
            try
            {
                //var eventAggregator = new EventAggregator();
                //var component = new Component(eventAggregator);

                //var i2cServiceBus = Mock.Of<II2CBusService>();
                //var logger = Mock.Of<ILogger>();
                ////Mock.Get(daylightService).Setup(x => x.Sunrise).Returns(TimeSpan.FromHours(8));

                //var adapter = new HSREL8Adapter(eventAggregator, i2cServiceBus, logger)
                //{
                //    Uid = "HSREL8Adapter"
                //};
                //adapter[AdapterProperties.I2cAddress] = new IntValue(100);

                //var adapterReference = new AdapterReference();
                //adapterReference[AdapterProperties.PinNumber] = new IntValue(1);

                //await adapter.Initialize().ConfigureAwait(false);

                //component.AddAdapter(adapterReference);

                //await component.Initialize().ConfigureAwait(false);
            }
            catch (System.Exception ee)
            {
                throw;
            }

            // Assert.AreEqual(true, lampDictionary[LivingroomId].GetIsTurnedOn());
        }
    }
}