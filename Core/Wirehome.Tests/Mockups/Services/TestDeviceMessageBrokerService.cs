using System;
using Wirehome.Contracts.Hardware.DeviceMessaging;
using Wirehome.Contracts.Hardware.Mqtt;
using Wirehome.Contracts.Services;

namespace Wirehome.Tests.Mockups.Services
{
    public class TestDeviceMessageBrokerService : ServiceBase, IDeviceMessageBrokerService
    {
        public event EventHandler<DeviceMessageReceivedEventArgs> MessageReceived;

        public void Publish(string topic, byte[] payload, MqttQosLevel qosLevel)
        {
            var deviceMessage = new DeviceMessage
            {
                Topic = topic,
                Payload = payload,
                QosLevel = qosLevel
            };

            MessageReceived?.Invoke(this, new DeviceMessageReceivedEventArgs(deviceMessage));
        }

        public void Subscribe(string topicPattern, Action<DeviceMessage> callback)
        {
        }
    }
}
