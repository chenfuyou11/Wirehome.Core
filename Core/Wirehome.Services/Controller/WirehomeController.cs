using AutoMapper;
using AutoMapper.Configuration;
using CSharpFunctionalExtensions;
using Quartz;
using Quartz.Spi;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Wirehome.ComponentModel.Adapters;
using Wirehome.ComponentModel.Commands;
using Wirehome.ComponentModel.Components;
using Wirehome.ComponentModel.Configuration;
using Wirehome.Core.EventAggregator;
using Wirehome.Core.Services;
using Wirehome.Core.Services.DependencyInjection;
using Wirehome.Core.Services.I2C;
using Wirehome.Core.Services.Logging;
using Wirehome.Core.Services.Quartz;
using Wirehome.Core.Services.Roslyn;
using Wirehome.Model.Extensions;

namespace Wirehome.Model.Core
{
    public class WirehomeController : Actor
    {
        private readonly IContainer _container;
        private readonly ControllerOptions _options;

        private ILogger _log;
        private IConfigurationService _confService;
        private IResourceLocatorService _resourceLocator;
        private IRoslynCompilerService _roslynCompilerService;
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
                await base.Initialize().ConfigureAwait(false);

                RegisterServices();

                LoadDynamicAdapters(_options.AdapterMode);

                await InitializeServices().ConfigureAwait(false);
                await InitializeConfiguration().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _log.Error(e, "Unhanded exception while application startup");
                throw;
            }
        }

        private void LoadDynamicAdapters(AdapterMode adapterMode)
        {
            _log.Info($"Loading adapters in mode: {adapterMode}");

            if (adapterMode == AdapterMode.Compiled)
            {
                var result = _roslynCompilerService.CompileAssemblies(_resourceLocator.GetRepositoyLocation());
                var veryfy = Result.Combine(result.ToArray());
                if (veryfy.IsFailure) throw new Exception($"Error while compiling adapters: {veryfy.Error}");

                foreach(var adapter in result)
                {
                    Assembly.LoadFrom(adapter.Value);
                }
            }
            else
            {
                _log.Info($"Using only build in adapters");
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
            _resourceLocator = _container.GetInstance<IResourceLocatorService>();
            _roslynCompilerService = _container.GetInstance<IRoslynCompilerService>();
        }

        private void RegisterNativeServices()
        {
            if (_options.NativeServicesRegistration == null) throw new Exception("Missing native services registration");

            _options.NativeServicesRegistration?.Invoke(_container);
        }

        private void RegisterBaseServices()
        {
            (_options.BaseServicesRegistration ?? RegisterBaseServices).Invoke(_container);
        }

        private void RegisterBaseServices(IContainer container)
        {
            container.RegisterCollection(_options.Loggers);

            container.RegisterSingleton<IEventAggregator, EventAggregator>();
            container.RegisterSingleton<IConfigurationService, ConfigurationService>();
            container.RegisterSingleton<II2CBusService, I2CBusService>();
            container.RegisterSingleton<ILogService, LogService>();
            container.RegisterSingleton<IResourceLocatorService, ResourceLocatorService>();
            container.RegisterSingleton<IAdapterServiceFactory, AdapterServiceFactory>();
            container.RegisterSingleton<ISerialMessagingService, SerialMessagingService>();
            container.RegisterSingleton<IRoslynCompilerService, RoslynCompilerService>();

            //Quartz
            container.RegisterSingleton<IJobFactory, SimpleInjectorJobFactory>();
            container.RegisterSingleton<ISchedulerFactory, SimpleInjectorSchedulerFactory>();
            container.RegisterSingleton(() => container.GetInstance<ISchedulerFactory>().GetScheduler().Result);

            //Auto mapper
            container.RegisterSingleton(GetMapper);
        }

        public IMapper GetMapper()
        {
            var mce = new MapperConfigurationExpression();
            mce.ConstructServicesUsing(_container.GetInstance);
            _resourceLocator = _container.GetInstance<IResourceLocatorService>();
            var profile = new WirehomeMappingProfile();
            mce.AddProfile(profile);


            return new Mapper(new MapperConfiguration(mce), t => _container.GetInstance(t));
        }

        private async Task InitializeConfiguration()
        {
            _homeConfiguration = _confService.ReadConfiguration(_options.AdapterMode);

            foreach(var adapter in _homeConfiguration.Adapters)
            {
                try
                {
                    await adapter.Initialize().ConfigureAwait(false);
                }
                catch (Exception e)
                {

                    _log.Error(e, $"Exception while initialization of adapter {adapter.Uid}");
                }
            }

            foreach (var component in _homeConfiguration.Components)
            {
                try
                {
                    await component.Initialize().ConfigureAwait(false);
                }
                catch (Exception e)
                {

                    _log.Error(e, $"Exception while initialization of component {component.Uid}");
                }
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
                    await service.Initialize().ConfigureAwait(false);
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