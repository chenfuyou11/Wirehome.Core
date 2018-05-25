using Moq;
using Quartz;
using Quartz.Spi;
using System.IO;
using System.Threading.Tasks;
using Wirehome.ComponentModel.Adapters;
using Wirehome.ComponentModel.Configuration;
using Wirehome.Core.EventAggregator;
using Wirehome.Core.Interface.Native;
using Wirehome.Core.Services;
using Wirehome.Core.Services.DependencyInjection;
using Wirehome.Core.Services.I2C;
using Wirehome.Core.Services.Logging;
using Wirehome.Core.Services.Quartz;
using Wirehome.Model.Core;

namespace Wirehome.Core.Tests.ComponentModel
{
    public class ControllerBuilder
    {
        private IContainer _container;
        private string _configuration;

        private ControllerOptions GetControllerOptions()
        {
            //@"W:\Projects\HA4IoT\Adapters\AdaptersContainer\bin\Debug\netstandard2.0
            var adaptersRepo = Path.Combine(Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\..\..")), @"Adapters\AdaptersContainer\bin\Debug\netstandard2.0");
            var options = new ControllerOptions { AdapterRepository = adaptersRepo };
            options.NativeServicesRegistration = RegisterRaspberryServices;
            options.BaseServicesRegistration = RegisterContainerServices;
            options.ConfigurationPath = $@"ComponentModel\SampleConfigs\{_configuration}.json";

            return options;
        }

        public ControllerBuilder WithConfiguration(string configuration)
        {
            _configuration = configuration;
            return this;
        }
        
        public WirehomeController Build()
        {
            return new WirehomeController(GetControllerOptions());
        }

        public async Task<(WirehomeController controller, IContainer container)> BuildAndRun()
        {
            var controller = Build();
            await controller.Initialize().ConfigureAwait(false);
            
            return (controller, _container);
        }

        private void RegisterRaspberryServices(IContainer container)
        {
            container.RegisterSingleton(Mock.Of<INativeGpioController>());
            container.RegisterSingleton(Mock.Of<INativeI2cBus>());
            container.RegisterSingleton(Mock.Of<INativeSerialDevice>());
            container.RegisterSingleton(Mock.Of<INativeSoundPlayer>());
            container.RegisterSingleton(Mock.Of<INativeStorage>());
            container.RegisterSingleton(Mock.Of<INativeTimerSerice>());
        }

        private void RegisterContainerServices(IContainer container, string adapterRepo)
        {
            _container = container;

            var serial = Mock.Of<ISerialMessagingService>();
            var i2cServiceBus = Mock.Of<II2CBusService>();
            var logService = Mock.Of<ILogService>();
            var logger = Mock.Of<ILogger>();
            Mock.Get(logService).Setup(x => x.CreatePublisher(It.IsAny<string>())).Returns(logger);

            container.RegisterSingleton<IEventAggregator, EventAggregator.EventAggregator>();
            container.RegisterSingleton<IConfigurationService, ConfigurationService>();
            container.RegisterInstance(i2cServiceBus);
            container.RegisterInstance(logService);
            container.RegisterInstance(serial);
            container.RegisterSingleton<IAdapterServiceFactory, AdapterServiceFactory>();
            container.RegisterSingleton<IHttpMessagingService, HttpMessagingService>();
            //container.RegisterSingleton<ISerialMessagingService, SerialMessagingService>();

            //Quartz
            container.RegisterSingleton<IJobFactory, SimpleInjectorJobFactory>();
            container.RegisterSingleton<ISchedulerFactory, SimpleInjectorSchedulerFactory>();

            //Auto mapper
            container.RegisterSingleton(() => container.GetInstance<MapperProvider>().GetMapper(adapterRepo));
        }
    }
}