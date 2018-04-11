using System;
using System.Threading.Tasks;
using Wirehome.Contracts.Core;
using Wirehome.Contracts.Notifications;
using Wirehome.Contracts.Resources;
using Wirehome.Contracts.Services;

namespace Wirehome.Core
{
    public class SystemEventsService : ServiceBase, ISystemEventsService
    {
        private readonly INotificationService _notificationService;
        private readonly IResourceService _resourceService;

        public SystemEventsService(IController controller, INotificationService notificationService, IResourceService resourceService)
        {
            if (controller == null) throw new ArgumentNullException(nameof(controller));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));

            controller.StartupCompleted += OnStartupCompleted;
            controller.StartupFailed += (s, e) => StartupFailed?.Invoke(this, EventArgs.Empty);
            _resourceService = resourceService ?? throw new ArgumentNullException(nameof(resourceService));
        }

        public override Task Initialize()
        {
            _resourceService.RegisterText(SystemEventNotification.Booted, "System is booted.");
            return Task.CompletedTask;
        }

        public event EventHandler StartupCompleted;
        public event EventHandler StartupFailed;

        private void OnStartupCompleted(object sender, EventArgs eventArgs)
        {
            _notificationService.CreateInformation(SystemEventNotification.Booted);
            StartupCompleted?.Invoke(this, EventArgs.Empty);
        }
    }
}
