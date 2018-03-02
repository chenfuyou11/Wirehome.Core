using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;
using Wirehome.ComponentModel.Adapters;
using Wirehome.ComponentModel.Capabilities;
using Wirehome.ComponentModel.Component;
using Wirehome.ComponentModel.ValueTypes;
using Wirehome.Core;
using Wirehome.Core.Communication.I2C;
using Wirehome.Core.EventAggregator;

namespace Wirehome.Extensions.Tests
{

    [TestClass]
    public class ComponentModelTests : ReactiveTest
    {
        [TestMethod]
        public async Task TestComponent()
        {
            try
            {
                var eventAggregator = new EventAggregator();
                var component = new Component(eventAggregator);

                var i2cServiceBus = Mock.Of<II2CBusService>();
                var logger = Mock.Of<ILogger>();
                //Mock.Get(daylightService).Setup(x => x.Sunrise).Returns(TimeSpan.FromHours(8));

                var adapter = new HSREL8Adapter(eventAggregator, i2cServiceBus, logger)
                {
                    Uid = "HSREL8Adapter"
                };
                adapter[AdapterProperties.I2cAddress] = new IntValue(100);

                var adapterReference = new AdapterReference();
                adapterReference[AdapterProperties.PinNumber] = new IntValue(1);
                
                await adapter.Initialize().ConfigureAwait(false);

                component.AddAdapter(adapterReference);

                await component.Initialize().ConfigureAwait(false);

            }
            catch (System.Exception ee)
            {

                throw;
            }
           

           // Assert.AreEqual(true, lampDictionary[LivingroomId].GetIsTurnedOn());
        }

        
    }
}
