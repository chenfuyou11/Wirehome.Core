using System;
using Wirehome.Contracts.Areas;
using Wirehome.Contracts.Components;
using Wirehome.Contracts.Core;
using Wirehome.Contracts.Environment;
using Wirehome.Contracts.Messaging;
using Wirehome.Contracts.Notifications;
using Wirehome.Contracts.Resources;
using Wirehome.Contracts.Scheduling;
using Wirehome.Contracts.Services;
using Wirehome.Contracts.Settings;

namespace Wirehome.Automations
{
    public class AutomationFactory
    {
        private readonly IMessageBrokerService _messageBroker;
        private readonly ISchedulerService _schedulerService;
        private readonly INotificationService _notificationService;
        private readonly IDateTimeService _dateTimeService;
        private readonly IDaylightService _daylightService;
        private readonly IOutdoorService _outdoorService;
        private readonly IComponentRegistryService _componentService;
        private readonly ISettingsService _settingsService;
        private readonly IResourceService _resourceService;

        public AutomationFactory(
            ISchedulerService schedulerService,
            INotificationService notificationService,
            IDateTimeService dateTimeService,
            IDaylightService daylightService,
            IOutdoorService outdoorService,
            IComponentRegistryService componentService,
            ISettingsService settingsService,
            IResourceService resourceService,
            IMessageBrokerService messageBroker)
        {
            _messageBroker = messageBroker ?? throw new ArgumentNullException(nameof(messageBroker));
            _schedulerService = schedulerService ?? throw new ArgumentNullException(nameof(schedulerService));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
            _daylightService = daylightService ?? throw new ArgumentNullException(nameof(daylightService));
            _outdoorService = outdoorService ?? throw new ArgumentNullException(nameof(outdoorService));
            _componentService = componentService ?? throw new ArgumentNullException(nameof(componentService));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _resourceService = resourceService ?? throw new ArgumentNullException(nameof(resourceService));
        }

        public ConditionalOnAutomation RegisterConditionalOnAutomation(IArea area, Enum id)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            var automation =
                new ConditionalOnAutomation(
                    $"{area.Id}.{id}",
                    _schedulerService,
                    _dateTimeService,
                    _daylightService);

            area.RegisterAutomation(automation);

            return automation;
        }

        public RollerShutterAutomation RegisterRollerShutterAutomation(IArea area, Enum id)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            var automation = new RollerShutterAutomation(
                $"{area.Id}.{id}",
                _notificationService,
                _schedulerService,
                _dateTimeService,
                _daylightService,
                _outdoorService,
                _componentService,
                _settingsService,
                _resourceService);

            area.RegisterAutomation(automation);

            return automation;
        }

        public TurnOnAndOffAutomation RegisterTurnOnAndOffAutomation(IArea area, Enum id)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            var automation =
                new TurnOnAndOffAutomation(
                    $"{area.Id}.{id}",
                    _dateTimeService,
                    _schedulerService,
                    _settingsService,
                    _daylightService,
                    _messageBroker);

            area.RegisterAutomation(automation);

            return automation;
        }
    }
}
