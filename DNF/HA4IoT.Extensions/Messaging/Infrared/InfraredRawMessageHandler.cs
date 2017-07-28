using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using Windows.Storage.Streams;

namespace HA4IoT.Extensions.Messaging
{
    public class InfraredRawMessageHandler : IMessageHandler
    {
        private const byte MESSAGE_TYPE = 4;

        public bool CanHandleI2C(string messageType)
        {
            return false;
        }

        public bool CanHandleUart(byte messageType, byte messageSize)
        {
            if (messageType == MESSAGE_TYPE)
            {
                return true;
            }

            return false;
        }

        public byte[] PrepareI2cPackage(JObject message)
        {
            throw new NotImplementedException();
        }

        public object ReadUart(DataReader reader, byte messageSize)
        {
            var arraySize = messageSize / 2;
            var buffer = new ushort[arraySize];
            for(int i=0; i < arraySize; i++)
            {
                buffer[i] = reader.ReadUInt16();
            }

            return new InfraredRawMessage
            {
                RawArray = buffer.ToList()
            };
        }

        public object ReadUart(IDataReader reader, byte messageSize)
        {
            throw new NotImplementedException();
        }

        public Type SupportedMessageType()
        {
            return typeof(InfraredRawMessage);
        }
    }
}
