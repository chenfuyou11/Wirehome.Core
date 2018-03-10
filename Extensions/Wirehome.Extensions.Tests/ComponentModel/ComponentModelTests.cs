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
using Wirehome.ComponentModel.Configuration;
using Wirehome.Core.Communication.I2C;
using Wirehome.Core.EventAggregator;
using Wirehome.Core.Services.DependencyInjection;
using Wirehome.Core.Services.Logging;
using Wirehome.Core.Services.Quartz;

namespace Wirehome.Extensions.Tests
{

    [TestClass]
    public class ComponentModelTests : ReactiveTest
    {

        [TestMethod]
        public async Task TestReadComponentsFromConfig()
        {
            var file = ReadConfig("componentConiguration");
            var container = PrepareContainer();

            var confService = container.GetInstance<IConfigurationService>();
            var components = confService.ReadConfiguration(file);

            foreach(var component in components)
            {
                await component.Initialize().ConfigureAwait(false);
            }
        }

        private IContainer PrepareContainer()
        {
            var reg = new WirehomeContainer(new ControllerOptions())
            {
                RegisterBaseServices = RegisterContainerServices
            };
            return reg.RegisterServices();

        }

        private void RegisterContainerServices(Container container)
        {
            var i2cServiceBus = Mock.Of<II2CBusService>();

            container.RegisterSingleton<IEventAggregator, EventAggregator>();
            container.RegisterSingleton<IConfigurationService, ConfigurationService>();
            container.RegisterSingleton(i2cServiceBus);
            container.RegisterSingleton<ILogService, LogService>();
            container.RegisterSingleton<IAdapterServiceFactory, AdapterServiceFactory>();

            //Quartz
            container.RegisterSingleton<IJobFactory, SimpleInjectorJobFactory>();
            container.RegisterSingleton<ISchedulerFactory, SimpleInjectorSchedulerFactory>();
            container.Register(() => container.GetInstance<ISchedulerFactory>().GetScheduler().Result);

            //Auto mapper
            container.RegisterSingleton(() => container.GetInstance<MapperProvider>().GetMapper());
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
