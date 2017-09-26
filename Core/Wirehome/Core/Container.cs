using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Wirehome.Actuators;
using Wirehome.Api;
using Wirehome.Api.Cloud.Azure;
using Wirehome.Api.Cloud.CloudConnector;
using Wirehome.Areas;
using Wirehome.Automations;
using Wirehome.Backup;
using Wirehome.Components;
using Wirehome.Configuration;
using Wirehome.Contracts.Api;
using Wirehome.Contracts.Areas;
using Wirehome.Contracts.Automations;
using Wirehome.Contracts.Backup;
using Wirehome.Contracts.Components;
using Wirehome.Contracts.Configuration;
using Wirehome.Contracts.Core;
using Wirehome.Contracts.Environment;
using Wirehome.Contracts.ExternalServices.TelegramBot;
using Wirehome.Contracts.ExternalServices.Twitter;
using Wirehome.Contracts.Hardware.DeviceMessaging;
using Wirehome.Contracts.Hardware.Interrupts;
using Wirehome.Contracts.Hardware.RemoteSockets;
using Wirehome.Contracts.Logging;
using Wirehome.Contracts.Messaging;
using Wirehome.Contracts.Notifications;
using Wirehome.Contracts.PersonalAgent;
using Wirehome.Contracts.Resources;
using Wirehome.Contracts.Scripting;
using Wirehome.Contracts.Settings;
using Wirehome.Contracts.Storage;
using Wirehome.Devices;
using Wirehome.Environment;
using Wirehome.ExternalServices.OpenWeatherMap;
using Wirehome.ExternalServices.TelegramBot;
using Wirehome.ExternalServices.Twitter;
using Wirehome.Hardware.Drivers.CCTools;
using Wirehome.Hardware.Drivers.Outpost;
using Wirehome.Hardware.Drivers.Sonoff;
using Wirehome.Hardware.Interrupts;
using Wirehome.Hardware.RemoteSockets;
using Wirehome.Health;
using Wirehome.Logging;
using Wirehome.Messaging;
using Wirehome.Notifications;
using Wirehome.PersonalAgent;
using Wirehome.Resources;
using Wirehome.Scripting;
using Wirehome.Sensors;
using Wirehome.Settings;
using Wirehome.Status;
using Wirehome.Storage;
using SimpleInjector;
using System.Threading.Tasks;

namespace Wirehome.Core
{
    public class Container : IContainer
    {
        private readonly SimpleInjector.Container _container = new SimpleInjector.Container();
        private readonly ControllerOptions _options;

        public Container(ControllerOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));

            _container.RegisterSingleton(options);
        }

        public void Verify()
        {
            _container.Verify();
        }

        public IList<InstanceProducer> GetCurrentRegistrations()
        {
            return _container.GetCurrentRegistrations().ToList();
        }

        public TContract GetInstance<TContract>() where TContract : class
        {
            return _container.GetInstance<TContract>();
        }

        public object GetInstance(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            return _container.GetInstance(type);
        }
        
        public IList<TContract> GetInstances<TContract>() where TContract : class
        {
            var services = new List<TContract>();

            foreach (var registration in _container.GetCurrentRegistrations())
            {
                if (typeof(TContract).IsAssignableFrom(registration.ServiceType))
                {
                    services.Add((TContract)registration.GetInstance());
                }
            }

            return services;
        }

        public void RegisterFactory<T>(Func<T> factory) where T : class
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            _container.Register(factory);
        }

        public void RegisterSingleton<TImplementation>() where TImplementation : class
        {
            _container.RegisterSingleton<TImplementation>();
        }

        public void RegisterSingleton<TContract, TImplementation>() where TContract : class where TImplementation : class, TContract
        {
            _container.RegisterSingleton<TContract, TImplementation>();
        }

        public void RegisterSingleton(Type service, Type implementation) 
        {
            _container.Register(service, implementation, Lifestyle.Singleton);
        }

        public void RegisterSingletonCollection<TItem>(IEnumerable<TItem> items) where TItem : class
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            _container.RegisterCollection(items);
        }

        public void RegisterSingletonCollection<TItem>(IEnumerable<Assembly> assemblies) where TItem : class
        {
            if (assemblies == null) throw new ArgumentNullException(nameof(assemblies));

            _container.RegisterCollection(typeof(TItem), assemblies);
        }

        public void RegisterSingleton<TContract>(Func<TContract> instanceCreator) where TContract : class
        {
            if (instanceCreator == null) throw new ArgumentNullException(nameof(instanceCreator));

            _container.RegisterSingleton(instanceCreator);
        }

        public void RegisterType<T>() where T : class
        {
            _container.Register<T>();
        }



        //TODO
        public void RegisterServices()
        {
            _container.RegisterSingleton<IContainer>(() => this);

            foreach (var customService in _options.CustomServices)
            {
                _container.RegisterSingleton(customService);
            }

            _container.RegisterCollection<ILogAdapter>(_options.LogAdapters);
            _container.RegisterSingleton<ILogService, LogService>();
            _container.RegisterSingleton<IHealthService, HealthService>();
            _container.RegisterSingleton<IDateTimeService, DateTimeService>();

            _container.RegisterSingleton<DiscoveryServerService>();

            _container.RegisterSingleton<IConfigurationService, ConfigurationService>();
            _container.RegisterInitializer<ConfigurationService>(s => s.Initialize());

            _container.RegisterSingleton<IStorageService, StorageService>();

            _container.RegisterSingleton<ISystemEventsService, SystemEventsService>();
            _container.RegisterSingleton<ISystemInformationService, SystemInformationService>();
            _container.RegisterSingleton<IBackupService, BackupService>();

            _container.RegisterSingleton<IResourceService, ResourceService>();
            _container.RegisterInitializer<ResourceService>(s => s.Initialize());

            _container.RegisterSingleton<IApiDispatcherService, ApiDispatcherService>();
            //_container.RegisterSingleton<HttpServerService>();
            _container.RegisterSingleton<AzureCloudService>();
            _container.RegisterSingleton<CloudConnectorService>();

            _container.RegisterSingleton<INotificationService, NotificationService>();
            _container.RegisterInitializer<NotificationService>(s => s.Initialize());

            _container.RegisterSingleton<ISettingsService, SettingsService>();
            _container.RegisterInitializer<SettingsService>(s => s.Initialize());

            //_container.RegisterSingleton<ISchedulerService, SchedulerService>();
            //_container.RegisterSingleton<ITimerService, TimerService>();
            // _container.RegisterSingleton<II2CBusService, I2CBusService>();
            //_container.RegisterSingleton<IGpioService, GpioService>();

            _container.RegisterSingleton<IMessageBrokerService, MessageBrokerService>();
            _container.RegisterSingleton<IInterruptMonitorService, InterruptMonitorService>();

            _container.RegisterSingleton<IDeviceMessageBrokerService, DeviceMessageBrokerService>();
            _container.RegisterInitializer<DeviceMessageBrokerService>(s => s.Initialize());

            _container.RegisterSingleton<IRemoteSocketService, RemoteSocketService>();

            _container.RegisterSingleton<CCToolsDeviceService>();
            _container.RegisterSingleton<SonoffDeviceService>();
            _container.RegisterSingleton<OutpostDeviceService>();

            _container.RegisterSingleton<IDeviceRegistryService, DeviceRegistryService>();
            _container.RegisterSingleton<IAreaRegistryService, AreaRegistryService>();
            _container.RegisterSingleton<IComponentRegistryService, ComponentRegistryService>();
            _container.RegisterSingleton<IAutomationRegistryService, AutomationRegistryService>();
            _container.RegisterSingleton<IScriptingService, ScriptingService>();

            _container.RegisterSingleton<ActuatorFactory>();
            _container.RegisterSingleton<SensorFactory>();
            _container.RegisterSingleton<AutomationFactory>();

            _container.RegisterSingleton<IPersonalAgentService, PersonalAgentService>();

            _container.RegisterSingleton<IOutdoorService, OutdoorService>();
            _container.RegisterSingleton<IDaylightService, DaylightService>();
            _container.RegisterSingleton<OpenWeatherMapService>();
            _container.RegisterSingleton<ControllerSlaveService>();

            _container.RegisterSingleton<ITwitterClientService, TwitterClientService>();
            _container.RegisterSingleton<ITelegramBotService, TelegramBotService>();

            _container.RegisterSingleton<IStatusService, StatusService>();
        }
    }
}
