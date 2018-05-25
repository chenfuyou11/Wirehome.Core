using Quartz;
using Quartz.Spi;
using System;
using System.Linq;
using System.Threading.Tasks;
using Wirehome.ComponentModel.Adapters;
using Wirehome.ComponentModel.Commands;
using Wirehome.ComponentModel.Components;
using Wirehome.ComponentModel.Configuration;
using Wirehome.Core.EventAggregator;
using Wirehome.Core.Extensions;
using Wirehome.Core.Services;
using Wirehome.Core.Services.DependencyInjection;
using Wirehome.Core.Services.I2C;
using Wirehome.Core.Services.Logging;
using Wirehome.Core.Services.Quartz;
using Wirehome.Model.Extensions;

namespace Wirehome.Model.Core
{
    public class WirehomeController : Actor
    {
        private readonly IContainer _container;
        private readonly ControllerOptions _options;

        private ILogger _log;
        private IConfigurationService _confService;
        private WirehomeConfiguration _homeConfiguration;

        public WirehomeController(ControllerOptions options)
        {
            _container = new WirehomeContainer();
            _options = options;
        }

        public override async Task Initialize()
        {
            try
            {
                await base.Initialize();

                RegisterServices();

                await InitializeServices();
                await InitializeConfiguration();
            }
            catch (Exception e)
            {
                _log.Error(e, "Unhanded exception while application startup");
                throw;
            }
        }

        private void RegisterServices()
        {
            _container.RegisterSingleton(() => _container);

            RegisterBaseServices();

            RegisterNativeServices();

            GetServicesFromContainer();

            _container.Verify();
        }

        private void GetServicesFromContainer()
        {
            _log = _container.GetInstance<ILogService>().CreatePublisher(nameof(WirehomeController));
            _confService = _container.GetInstance<IConfigurationService>();
        }

        private void RegisterNativeServices()
        {
            if (_options.NativeServicesRegistration == null) throw new Exception("Missing native services registration");

            _options.NativeServicesRegistration?.Invoke(_container);
        }

        private void RegisterBaseServices()
        {
            (_options.BaseServicesRegistration ?? RegisterBaseServices).Invoke(_container, _options.AdapterRepository);
        }

        private void RegisterBaseServices(IContainer container, string adapterRepo)
        {
            container.RegisterSingleton<IEventAggregator, EventAggregator>();
            container.RegisterSingleton<IConfigurationService, ConfigurationService>();
            container.RegisterSingleton<II2CBusService, I2CBusService>();
            container.RegisterSingleton<ILogService, LogService>();
            container.RegisterSingleton<IAdapterServiceFactory, AdapterServiceFactory>();

            //Quartz
            container.RegisterSingleton<IJobFactory, SimpleInjectorJobFactory>();
            container.RegisterSingleton<ISchedulerFactory, SimpleInjectorSchedulerFactory>();
            container.RegisterSingleton(() => container.GetInstance<ISchedulerFactory>().GetScheduler().Result);

            //Auto mapper
            container.RegisterInstance(container.GetInstance<MapperProvider>().GetMapper(adapterRepo));
        }



        private async Task InitializeConfiguration()
        {
            _homeConfiguration = _confService.ReadConfiguration(_options.ConfigurationPath, _options.AdapterRepository);

            foreach(var adapter in _homeConfiguration.Adapters)
            {
                await adapter.Initialize();
            }

            foreach (var component in _homeConfiguration.Components)
            {
                await component.Initialize();
            }
        }

        private async Task InitializeServices()
        {
            var services = _container.GetSerives();

            while (services.Count > 0)
            {
                var service = services.Dequeue();
                try
                {
                    await service.Initialize();
                }
                catch (Exception exception)
                {
                    _log.Error(exception, $"Error while starting service '{service.GetType().Name}'. " + exception.Message);
                }
            }
        }

        protected override void LogException(Exception ex) => _log.Error(ex, $"Unhanded controller exception");

        protected Component GetComponentCommandHandler(Command command)
        {
            var uid = command.GetPropertyValue(CommandProperties.DeviceUid);
            if (uid.HasNoValue) throw new ArgumentException($"Command GetComponentCommand is missing [{CommandProperties.DeviceUid}] property");
            
            return _homeConfiguration.Components.FirstOrDefault(c => c.Uid == uid.Value.ToStringValue());
        }
    }
}