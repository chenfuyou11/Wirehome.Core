using System;
using Wirehome.Api;
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
using Wirehome.Contracts.Hardware.DeviceMessaging;
using Wirehome.Contracts.Logging;
using Wirehome.Contracts.Messaging;
using Wirehome.Contracts.Notifications;
using Wirehome.Contracts.Resources;
using Wirehome.Contracts.Scheduling;
using Wirehome.Contracts.Scripting;
using Wirehome.Contracts.Services;
using Wirehome.Contracts.Settings;
using Wirehome.Contracts.Storage;
using Wirehome.Core;
using Wirehome.Logging;
using Wirehome.Messaging;
using Wirehome.Notifications;
using Wirehome.Resources;
using Wirehome.Scheduling;
using Wirehome.Scripting;
using Wirehome.Settings;
using Wirehome.Tests.Mockups.Services;
using Newtonsoft.Json.Linq;

namespace Wirehome.Tests.Mockups
{
    public class TestController : IController
    {
        private readonly TestApiAdapter _apiAdapter = new TestApiAdapter();
        private readonly Container _container;

        public TestController()
        {
            var options = new ControllerOptions();
            _container = new Container(options);
            _container.RegisterCollection(options.LogAdapters);
            _container.RegisterSingleton<IController>(() => this);
            _container.RegisterSingleton<ILogService, LogService>();
            _container.RegisterSingleton<IBackupService, BackupService>();
            _container.RegisterSingleton<IStorageService, TestStorageService>();
            _container.RegisterSingleton<ISettingsService, SettingsService>();
            _container.RegisterSingleton<IApiDispatcherService, ApiDispatcherService>();
            _container.RegisterSingleton<ISystemInformationService, SystemInformationService>();
            _container.RegisterSingleton<ITimerService, TestTimerService>();
            _container.RegisterSingleton<IDaylightService, TestDaylightService>();
            _container.RegisterSingleton<IDateTimeService, TestDateTimeService>();
            _container.RegisterSingleton<IResourceService, ResourceService>();
            //TODO
            //_container.RegisterSingleton<ISchedulerService, SchedulerService>();
            _container.RegisterSingleton<INotificationService, NotificationService>();
            _container.RegisterSingleton<ISystemEventsService, SystemEventsService>();
            _container.RegisterSingleton<IAutomationRegistryService, AutomationRegistryService>();
            _container.RegisterSingleton<IComponentRegistryService, ComponentRegistryService>();
            _container.RegisterSingleton<IAreaRegistryService, AreaRegistryService>();
            _container.RegisterSingleton<IDeviceMessageBrokerService, TestDeviceMessageBrokerService>();
            _container.RegisterSingleton<IScriptingService, ScriptingService>();
            _container.RegisterSingleton<IMessageBrokerService, MessageBrokerService>();
            _container.RegisterSingleton<IConfigurationService, ConfigurationService>();

            _container.Verify();

            var logService = _container.GetInstance<ILogService>();
            var log = logService.CreatePublisher(nameof(TestController));

            _container.StartupServices(log);
            _container.ExposeRegistrationsToApi();

            _container.GetInstance<IApiDispatcherService>().RegisterAdapter(_apiAdapter);
        }

        public event EventHandler<StartupCompletedEventArgs> StartupCompleted;
        public event EventHandler<StartupFailedEventArgs> StartupFailed;
        
        public void RaiseStartupCompleted()
        {
            StartupCompleted?.Invoke(this, new StartupCompletedEventArgs(TimeSpan.Zero));
        }

        public void RaiseStartupFailed()
        {
            StartupFailed?.Invoke(this, new StartupFailedEventArgs(TimeSpan.Zero, new Exception()));
        }

        public TInstance GetInstance<TInstance>() where TInstance : class
        {
            return _container.GetInstance<TInstance>();
        }

        public void SetTime(TimeSpan value)
        {
            ((TestDateTimeService)GetInstance<IDateTimeService>()).SetTime(value);
        }

        public void Tick(TimeSpan elapsedTime)
        {
            ((TestTimerService)GetInstance<ITimerService>()).ExecuteTick(elapsedTime);
        }

        public void AddComponent(IComponent component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            GetInstance<IComponentRegistryService>().RegisterComponent(component);
        }

        public IApiCall InvokeApi(string action, JObject parameter)
        {
            return _apiAdapter.Invoke(action, parameter);
        }
    }
}
