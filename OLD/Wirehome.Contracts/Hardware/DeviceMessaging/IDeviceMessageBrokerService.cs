using System;
using Wirehome.Contracts.Hardware.Mqtt;
using Wirehome.Contracts.Services;

namespace Wirehome.Contracts.Hardware.DeviceMessaging
{
    public interface IDeviceMessageBrokerService : IService
    {
        event EventHandler<DeviceMessageReceivedEventArgs> MessageReceived;

        void Publish(string topic, byte[] payload, MqttQosLevel qosLevel);

        void Subscribe(string topicPattern, Action<DeviceMessage> callback);
    }
}
