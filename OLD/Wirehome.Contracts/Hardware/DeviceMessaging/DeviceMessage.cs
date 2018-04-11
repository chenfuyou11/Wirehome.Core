using Wirehome.Contracts.Hardware.Mqtt;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Wirehome.Contracts.Hardware.DeviceMessaging
{
    public class DeviceMessage
    {
        public string Topic { get; set; }

        public byte[] Payload { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public MqttQosLevel QosLevel { get; set; }
    }
}
