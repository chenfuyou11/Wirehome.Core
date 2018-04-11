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

                await InitializeServices().ConfigureAwait(false);
                
                ExposeRegistrationsToApi();

                await TryConfigureAsync().ConfigureAwait(false);

                StartupCompleted?.Invoke(this, new StartupCompletedEventArgs(stopwatch.Elapsed));

                ExecuteStartupScripts();
            }
            catch (Exception exception)
            {
                StartupFailed?.Invoke(this, new StartupFailedEventArgs(stopwatch.Elapsed, exception));
                return false;
            }

            return true;
        }

        private void ExecuteStartupScripts()
        {
            _container.GetInstance<IScriptingService>().TryExecuteStartupScripts();
        }

        private void ExposeRegistrationsToApi()
        {
            _container.ExposeRegistrationsToApi();
        }

        private Task InitializeServices()
        {
            return _container.StartupServices(_log);
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

            _container.RegisterSingleton<DiscoveryServerService>();
            _container.RegisterSingleton<AzureCloudService>();
            _container.RegisterSingleton<CloudConnectorService>();
            _container.RegisterSingleton<CCToolsDeviceService>();
            _container.RegisterSingleton<SonoffDeviceService>();
            _container.RegisterSingleton<OutpostDeviceService>();
            _container.RegisterSingleton<ActuatorFactory>();
            _container.RegisterSingleton<SensorFactory>();
            _container.RegisterSingleton<AutomationFactory>();
            _container.RegisterSingleton<OpenWeatherMapService>();
            _container.RegisterSingleton<ControllerSlaveService>();

            _container.RegisterService<IConfigurationService, ConfigurationService>(100);
            _container.RegisterService<IInterruptMonitorService, InterruptMonitorService>(90);
            _container.RegisterService<IDeviceRegistryService, DeviceRegistryService>(80);
            _container.RegisterService<IRemoteSocketService, RemoteSocketService>(70);
            _container.RegisterService<IResourceService, ResourceService>(60);
            _container.RegisterService<INotificationService, NotificationService>(50);
            _container.RegisterService<ISettingsService, SettingsService>(40);
            _container.RegisterService<IDeviceMessageBrokerService, DeviceMessageBrokerService>(30);
            _container.RegisterService<ILogService, LogService>();
            _container.RegisterService<IHealthService, HealthService>();
            _container.RegisterService<IDateTimeService, DateTimeService>();
            _container.RegisterService<ITimerService, TimerService>();
            _container.RegisterService<IStorageService, StorageService>();
            _container.RegisterService<ISystemEventsService, SystemEventsService>();
            _container.RegisterService<ISystemInformationService, SystemInformationService>();
            _container.RegisterService<IBackupService, BackupService>();
            _container.RegisterService<IApiDispatcherService, ApiDispatcherService>();
            _container.RegisterService<ISchedulerService, SchedulerService>();
            _container.RegisterService<IMessageBrokerService, MessageBrokerService>();
            _container.RegisterService<IGpioService, GpioService>();
            _container.RegisterService<II2CBusService, I2CBusService>();
            _container.RegisterService<IAreaRegistryService, AreaRegistryService>();
            _container.RegisterService<IComponentRegistryService, ComponentRegistryService>();
            _container.RegisterService<IAutomationRegistryService, AutomationRegistryService>();
            _container.RegisterService<IScriptingService, ScriptingService>();
            _container.RegisterService<IPersonalAgentService, PersonalAgentService>();
            _container.RegisterService<IOutdoorService, OutdoorService>();
            _container.RegisterService<IDaylightService, DaylightService>();
            _container.RegisterService<ITwitterClientService, TwitterClientService>();
            _container.RegisterService<ITelegramBotService, TelegramBotService>();
            _container.RegisterService<IStatusService, StatusService>();
            _container.RegisterService<IHttpServerService, HttpServerService>();
        }
    }
}

