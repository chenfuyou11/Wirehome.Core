using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;
using Wirehome.ComponentModel.Components;
using Wirehome.Core.ComponentModel.Configuration;
using System.Linq;
using SimpleInjector;
using AutoMapper;
using Wirehome.Core.DI;
using System.Collections.Generic;

namespace Wirehome.Extensions.Tests
{

    [TestClass]
    public class ComponentModelTests : ReactiveTest
    {

        [TestMethod]
        public async Task TestReadComponentsFromConfig()
        {
            var file = ReadConfig("componentConiguration");

            var result = JsonConvert.DeserializeObject<WirehomeConfig>(file);
            //var factory = new ComponentFactory(new EventAggregator());

            var container = new Container();
            var reg = new Registrar();
            reg.Register(container);

            var mapper = container.GetInstance<IMapper>();
            var component = mapper.Map<IList<ComponentDTO>, IList<Component>>(result.Wirehome.Components);

            //var component = factory.Create(result.Wirehome.Components.FirstOrDefault());

            

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

        private string ReadConfig(string message) => File.ReadAllText($@"ComponentModel\SampleConfigs\{message}.json");



    }
}
