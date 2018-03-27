using Moq;
using Quartz;
using Quartz.Spi;
using SimpleInjector;
using System.IO;
using System.Threading.Tasks;
using Wirehome.ComponentModel.Adapters;
using Wirehome.ComponentModel.Configuration;
using Wirehome.Core.Communication.I2C;
using Wirehome.Core.EventAggregator;
using Wirehome.Core.Services.DependencyInjection;
using Wirehome.Core.Services.Logging;
using Wirehome.Core.Services.Quartz;

namespace Wirehome.Core.Tests.ComponentModel
{
    public static class CommonIntegrationcs
    {
        public static string ReadConfig(string message) => File.ReadAllText($@"ComponentModel\SampleConfigs\{message}.json");

        public static async Task<(WirehomeConfiguration config, IContainer container)> ReadConfiguration(string configName)
        {
            var file = CommonIntegrationcs.ReadConfig(configName);
            var container = CommonIntegrationcs.PrepareContainer();
            var confService = container.GetInstance<IConfigurationService>();
            var configuration = await confService.ReadConfiguration(file).ConfigureAwait(false);

            return (configuration, container);
        }

        public static IContainer PrepareContainer()
        {
            var reg = new WirehomeContainer(new ControllerOptions())
            {
                RegisterBaseServices = RegisterContainerServices
            };
            return reg.RegisterServices();
        }

        private static void RegisterContainerServices(Container container)
        {
            var i2cServiceBus = Mock.Of<II2CBusService>();

            container.RegisterSingleton<IEventAggregator, EventAggregator.EventAggregator>();
            container.RegisterSingleton<IConfigurationService, ConfigurationService>();
            container.RegisterSingleton(i2cServiceBus);
            container.RegisterSingleton<ILogService, LogService>();
            container.RegisterSingleton<IAdapterServiceFactory, AdapterServiceFactory>();

            //Quartz
            container.RegisterSingleton<IJobFactory, SimpleInjectorJobFactory>();
            container.RegisterSingleton<ISchedulerFactory, SimpleInjectorSchedulerFactory>();

            //Auto mapper
            container.RegisterSingleton(() => container.GetInstance<MapperProvider>().GetMapper());
        }
    }
}