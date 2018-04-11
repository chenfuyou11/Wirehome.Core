using System;
using System.Text;
using Wirehome.Contracts.Components.Adapters;
using Wirehome.Contracts.Hardware.DeviceMessaging;
using Wirehome.Contracts.Logging;

namespace Wirehome.Components.Adapters.MqttBased
{
    public class MqttBasedNumericSensorAdapter : INumericSensorAdapter
    {
        private readonly string _topic;
        private readonly ILogger _log;

        public MqttBasedNumericSensorAdapter(string topic, IDeviceMessageBrokerService deviceMessageBrokerService, ILogService logService)
        {
            if (deviceMessageBrokerService == null) throw new ArgumentNullException(nameof(deviceMessageBrokerService));

            _log = logService?.CreatePublisher(nameof(MqttBasedNumericSensorAdapter)) ?? throw new ArgumentNullException(nameof(logService));

            _topic = topic ?? throw new ArgumentNullException(nameof(topic));
            deviceMessageBrokerService.Subscribe(topic, ForwardValue);
        }

        public event EventHandler<NumericSensorAdapterValueChangedEventArgs> ValueChanged;

        public void Refresh()
        {
        }

        private void ForwardValue(DeviceMessage deviceMessage)
        {
            var payload = Encoding.UTF8.GetString(deviceMessage.Payload);

            float value;
            if (!float.TryParse(payload, out value))
            {
                _log.Warning($"Unable to parse MQTT payload '{payload}' of topic '{_topic}' to numeric value.");
                return;
            }

            ValueChanged?.Invoke(this, new NumericSensorAdapterValueChangedEventArgs(value));
        }
    }
}
