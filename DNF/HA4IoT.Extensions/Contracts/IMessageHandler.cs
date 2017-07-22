using Newtonsoft.Json.Linq;
using System;
using Windows.Storage.Streams;

namespace HA4IoT.Extensions
{
    public interface IMessageHandler
    {
        bool CanHandleUart(byte messageType, byte messageSize);
        object ReadUart(IDataReader reader, byte messageSize);

        Type SupportedMessageType();
        bool CanHandleI2C(string messageType);
        byte[] PrepareI2cPackage(JObject message);
    }
}