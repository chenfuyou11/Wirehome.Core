using System;
using System.Text;
using System.Threading.Tasks;
using Wirehome.Contracts.Components.Adapters;
using Wirehome.Contracts.Hardware;
using Wirehome.Contracts.Hardware.DeviceMessaging;
using Wirehome.Contracts.Hardware.Mqtt;
using Wirehome.Contracts.Logging;

namespace Wirehome.Components.Adapters.MqttBased
{
    public class MqttBasedBinaryOutputAdapter : IBinaryOutputAdapter
    {
        private readonly IDeviceMessageBrokerService _deviceMessageBrokerService;
        private readonly string _topic;
        private readonly ILogger _log;

        public MqttBasedBinaryOutputAdapter(string topic, IDeviceMessageBrokerService deviceMessageBrokerService, ILogService logService)
        {
            _deviceMessageBrokerService = deviceMessageBrokerService ?? throw new ArgumentNullException(nameof(deviceMessageBrokerService));
            _log = logService?.CreatePublisher(nameof(MqttBasedButtonAdapter)) ?? throw new ArgumentNullException(nameof(logService));

            _topic = topic ?? throw new ArgumentNullException(nameof(topic));
        }

        public Task SetState(AdapterPowerState powerState, params IHardwareParameter[] parameters)
        {
            _deviceMessageBrokerService.Publish(_topic, Encoding.UTF8.GetBytes(powerState.ToString()), MqttQosLevel.AtMostOnce);
            return Task.FromResult(0);
        }
    }
}
