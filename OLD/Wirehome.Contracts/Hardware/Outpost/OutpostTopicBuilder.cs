using System;

namespace Wirehome.Contracts.Hardware.Outpost
{
    public static class OutpostTopicBuilder
    {
        public static string BuildCommandTopic(string deviceName, string command)
        {
            if (deviceName == null) throw new ArgumentNullException(nameof(deviceName));
            if (command == null) throw new ArgumentNullException(nameof(command));

            return $"Wirehome/Device/{deviceName}/Command/{command}";
        }

        public static string BuildNotificationTopic(string deviceName, string notification)
        {
            if (deviceName == null) throw new ArgumentNullException(nameof(deviceName));
            if (notification == null) throw new ArgumentNullException(nameof(notification));

            return $"Wirehome/Device/{deviceName}/Notification/{notification}";
        }
    }
}
