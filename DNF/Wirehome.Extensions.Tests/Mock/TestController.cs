using System;
using HA4IoT.Api;
using HA4IoT.Core;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;
using HA4IoT.Logging;
using HA4IoT.Notifications;
using HA4IoT.Settings;
using HA4IoT.Contracts.Storage;
using HA4IoT.Contracts.Notifications;
using HA4IoT.Contracts.Resources;
using HA4IoT.Contracts.Settings;
using HA4IoT.Contracts.Backup;
using HA4IoT.Backup;
using HA4IoT.Resources;
using HA4IoT.Automations;
using HA4IoT.Components;
using HA4IoT.Areas;

namespace HA4IoT.Extensions.Tests
{
    public class TestController : IController
    {
        private readonly Container _container = new Container(new ControllerOptions());

        public TestController()
        {
            _container.RegisterSingleton<IController>(() => this);
            _container.RegisterSingleton<ILogService, LogService>();
            _container.RegisterSingleton<IBackupService, BackupService>();
            _container.RegisterSingleton<IStorageService, TestStorageService>();
            _container.RegisterSingleton<ISettingsService, SettingsService>();
            _container.RegisterSingleton<IApiDispatcherService, ApiDispatcherService>();
            _container.RegisterSingleton<ISystemInformationService, SystemInformationService>();
            _container.RegisterSingleton<ITimerService, TestTimerService>();
            //_container.RegisterSingleton<IDaylightService, TestDaylightService>();
            _container.RegisterSingleton<IDateTimeService, TestDateTimeService>();
            _container.RegisterSingleton<IResourceService, ResourceService>();
           // _container.RegisterSingleton<ISchedulerService, SchedulerService>();
            _container.RegisterSingleton<INotificationService, NotificationService>();
            _container.RegisterSingleton<ISystemEventsService, SystemEventsService>();
            _container.RegisterSingleton<IAutomationRegistryService, AutomationRegistryService>();
            _container.RegisterSingleton<IComponentRegistryService, ComponentRegistryService>();
            _container.RegisterSingleton<IAreaRegistryService, AreaRegistryService>();
           // _container.RegisterSingleton<IDeviceMessageBrokerService, TestDeviceMessageBrokerService>();

            _container.Verify();

            var logService = _container.GetInstance<ILogService>();
            var log = logService.CreatePublisher(nameof(TestController));

            _container.StartupServices(log);
            _container.ExposeRegistrationsToApi();

            StartupCompleted?.Invoke(null, null);
            StartupFailed?.Invoke(null, null);
        }

        private void TestController_StartupFailed(object sender, StartupFailedEventArgs e)
        {
            
        }

        private void TestController_StartupCompleted(object sender, StartupCompletedEventArgs e)
        {
            
        }

        public event EventHandler<StartupCompletedEventArgs> StartupCompleted;
        public event EventHandler<StartupFailedEventArgs> StartupFailed;

        public TInstance GetInstance<TInstance>() where TInstance : class
        {
            return _container.GetInstance<TInstance>();
        }

        public void SetTime(TimeSpan value)
        {
            
        }

        public void Tick(TimeSpan elapsedTime)
        {
           
        }

        public void AddComponent(IComponent component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            GetInstance<IComponentRegistryService>().RegisterComponent(component);
        }

    }
}
