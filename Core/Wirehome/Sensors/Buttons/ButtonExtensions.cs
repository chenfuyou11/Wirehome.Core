using System;
using Wirehome.Contracts.Areas;
using Wirehome.Contracts.Messaging;
using Wirehome.Contracts.Sensors;
using Wirehome.Contracts.Sensors.Events;
using Wirehome.Contracts.Triggers;
using Wirehome.Triggers;

namespace Wirehome.Sensors.Buttons
{
    public static class ButtonExtensions
    {
        public static IButton GetButton(this IArea area, Enum id)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            return area.GetComponent<IButton>($"{area.Id}.{id}");
        }

        public static ITrigger CreatePressedShortTrigger(this IButton button, IMessageBrokerService messageBroker)
        {
            if (button == null) throw new ArgumentNullException(nameof(button));
            if (messageBroker == null) throw new ArgumentNullException(nameof(messageBroker));

            return messageBroker.CreateTrigger<ButtonPressedShortEvent>(button.Id);
        }

        public static ITrigger CreatePressedLongTrigger(this IButton button, IMessageBrokerService messageBroker)
        {
            if (button == null) throw new ArgumentNullException(nameof(button));
            if (messageBroker == null) throw new ArgumentNullException(nameof(messageBroker));

            return messageBroker.CreateTrigger<ButtonPressedLongEvent>(button.Id);
        }
    }
}
