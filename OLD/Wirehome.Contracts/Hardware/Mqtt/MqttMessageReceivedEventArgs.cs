using System;

namespace Wirehome.Contracts.Hardware.Mqtt
{
    public class MqttMessageReceivedEventArgs : EventArgs
    {
        public string Topic { get; set; }

        public byte[] Payload { get; set; }

        public byte QosLevel { get; set; }
    }
}
