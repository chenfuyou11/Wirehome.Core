using System;
using System.Threading.Tasks;
using Wirehome.Contracts.Components.Adapters;
using Wirehome.Contracts.Hardware;
using Wirehome.Contracts.Hardware.DeviceMessaging;
using Wirehome.Contracts.Hardware.Mqtt;

namespace Wirehome.Hardware.Drivers.Sonoff
{
    public class SonoffBinaryOutputAdapter : IBinaryOutputAdapter
    {
        private readonly string _topic;
        private readonly IDeviceMessageBrokerService _deviceMessageBrokerService;

        public SonoffBinaryOutputAdapter(string topic, IDeviceMessageBrokerService deviceMessageBrokerService)
        {
            _topic = topic ?? throw new ArgumentNullException(nameof(topic));
            _deviceMessageBrokerService = deviceMessageBrokerService ?? throw new ArgumentNullException(nameof(deviceMessageBrokerService));
        }

        public Task SetState(AdapterPowerState powerState, params IHardwareParameter[] parameters)
        {
            _deviceMessageBrokerService.Publish(_topic, powerState == AdapterPowerState.On ? "ON" : "OFF", MqttQosLevel.ExactlyOnce);
            return Task.FromResult(0);
        }
    }
}