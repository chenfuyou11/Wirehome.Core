using System;
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
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using Wirehome.Contracts.Scheduling;
using Wirehome.Scheduling;
using Wirehome.Contracts.Hardware.Gpio;
using Wirehome.Hardware.Drivers.Gpio;
using Wirehome.Hardware.I2C;

namespace Wirehome.Core
{
    public class WirehomeController : IController
    {
        private readonly Container _container;
        private readonly ControllerOptions _options;
        private ILogger _log;
        private IContainer Container => _container;

        public event EventHandler<StartupCompletedEventArgs> StartupCompleted;
        public event EventHandler<StartupFailedEventArgs> StartupFailed;
   
        public WirehomeController(ControllerOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _container = new Container(options);
        }

        public Task<bool> RunAsync() => Task.Run(() => RunAsyncInternal());

        private async Task<bool> RunAsyncInternal()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                InitializeContainer();

                InitializeLogger();

                InitializeStorage();

                RegisterDevices();

                await _container.StartupServices(_log).ConfigureAwait(false);

                _container.ExposeRegistrationsToApi();

                await TryConfigureAsync().ConfigureAwait(false);

                StartupCompleted?.Invoke(this, new StartupCompletedEventArgs(stopwatch.Elapsed));

                _container.GetInstance<IScriptingService>().TryExecuteStartupScripts();
            }
            catch (Exception exception)
            {
                StartupFailed?.Invoke(this, new StartupFailedEventArgs(stopwatch.Elapsed, exception));
                return false;
            }

            return true;
        }

        private void RegisterDevices()
        {
            _container.GetInstance<IInterruptMonitorService>().RegisterInterrupts();
            _container.GetInstance<IDeviceRegistryService>().RegisterDevices();
            _container.GetInstance<IRemoteSocketService>().RegisterRemoteSockets();
        }

        private void InitializeStorage()
        {
            var nativeStorage = _container.GetInstance<INativeStorage>();
            StoragePath.Initialize(nativeStorage.LocalFolderPath(), nativeStorage.LocalFolderPath());
        }

        private void InitializeLogger()
        {
            _log = _container.GetInstance<ILogService>().CreatePublisher(nameof(WirehomeController));
        }

        private void InitializeContainer()
        {
            _container.RegisterSingleton<IController>(() => this);
            RegisterServices();
            _options.ContainerConfigurator?.ConfigureContainer(_container);

            if (!_container.ChackNativeImpelentationExists())
            {
                throw new Exception("Containter don't have implementations for native devices");
            }
            
            _container.Verify();
        }

        private async Task TryConfigureAsync()
        {
            try
            {
                _container.GetInstance<IApiDispatcherService>().ConfigurationRequested += (s, e) =>
                {
                    e.ApiContext.Result["Controller"] = JObject.FromObject(_container.GetInstance<ISettingsService>().GetSettings<ControllerSettings>());
                };

                await TryApplyCodeConfigurationAsync().ConfigureAwait(false);

                _log.Info("Resetting all components");
                var componentRegistry = _container.GetInstance<IComponentRegistryService>();
                foreach (var component in componentRegistry.GetComponents())
                {
                    component.TryReset();
                }

            }
            catch (Exception exception)
            {
                _log.Error(exception, "Error while configuring");
                _container.GetInstance<INotificationService>().CreateError("Error while configuring.");
            }
        }

        private async Task TryApplyCodeConfigurationAsync()
        {
            try
            {
                if (_options.ConfigurationType == null)
                {
                    _log.Verbose("No configuration type is set.");
                    return;
                }

                var configuration = _container.GetInstance(_options.ConfigurationType) as IConfiguration;
                if (configuration == null)
                {
                    _log.Warning("Configuration is set but does not implement 'IConfiguration'.");
                    return;
                }

                _log.Info("Applying configuration");
                await configuration.ApplyAsync();
            }
            catch (Exception exception)
            {
                _log.Error(exception, "Error while applying code configuration");
                _container.GetInstance<INotificationService>().CreateError("Configuration code has failed.");
            }
        }

        public void RegisterServices()
        {
            _container.RegisterSingleton<IContainer>(() => _container);

            foreach (var customService in _options.CustomServices)
            {
                _container.RegisterSingleton(customService);
            }

            _container.RegisterCollection<ILogAdapter>(_options.LogAdapters);
            _container.RegisterSingleton<ILogService, LogService>();
            _container.RegisterSingleton<IHealthService, HealthService>();
            _container.RegisterSingleton<IDateTimeService, DateTimeService>();
            _container.RegisterSingleton<ITimerService, TimerService>();

            _container.RegisterSingleton<DiscoveryServerService>();

            _container.RegisterSingleton<IConfigurationService, ConfigurationService>();
           // _container.RegisterInitializer<ConfigurationService>(s => s.Initialize());

            _container.RegisterSingleton<IStorageService, StorageService>();

            _container.RegisterSingleton<ISystemEventsService, SystemEventsService>();
            _container.RegisterSingleton<ISystemInformationService, SystemInformationService>();
            _container.RegisterSingleton<IBackupService, BackupService>();

            _container.RegisterSingleton<IResourceService, ResourceService>();
            //_container.RegisterInitializer<ResourceService>(s => s.Initialize());

            _container.RegisterSingleton<IApiDispatcherService, ApiDispatcherService>();
            _container.RegisterSingleton<AzureCloudService>();
            _container.RegisterSingleton<CloudConnectorService>();

            _container.RegisterSingleton<INotificationService, NotificationService>();
            //_container.RegisterInitializer<NotificationService>(s => s.Initialize());

            _container.RegisterSingleton<ISettingsService, SettingsService>();
            //_container.RegisterInitializer<SettingsService>(s => s.Initialize());
            _container.RegisterSingleton<ISchedulerService, SchedulerService>();

            _container.RegisterSingleton<IMessageBrokerService, MessageBrokerService>();
            _container.RegisterSingleton<IInterruptMonitorService, InterruptMonitorService>();
            _container.RegisterSingleton<IGpioService, GpioService>();
            _container.RegisterSingleton<II2CBusService, I2CBusService>();
            
            _container.RegisterSingleton<IDeviceMessageBrokerService, DeviceMessageBrokerService>();
            //_container.RegisterInitializer<DeviceMessageBrokerService>(s => s.Initialize());

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
            _container.RegisterSingleton<IHttpServerService, HttpServerService>();
            
        }
    }
}

