using System;
using Wirehome.Contracts.Core;
using Wirehome.Contracts.Notifications;
using Wirehome.Contracts.Resources;
using Wirehome.Contracts.Services;

namespace Wirehome.Core
{
    public class SystemEventsService : ServiceBase, ISystemEventsService
    {
        private readonly INotificationService _notificationService;

        public SystemEventsService(IController controller, INotificationService notificationService, IResourceService resourceService)
        {
            if (controller == null) throw new ArgumentNullException(nameof(controller));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));

            controller.StartupCompleted += OnStartupCompleted;
            controller.StartupFailed += (s, e) => StartupFailed?.Invoke(this, EventArgs.Empty);

            resourceService.RegisterText(SystemEventNotification.Booted, "System is booted.");
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
