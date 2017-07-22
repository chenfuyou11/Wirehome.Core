using System;
using Newtonsoft.Json.Linq;
using Windows.Storage.Streams;

namespace HA4IoT.Extensions
{
    public class LPD433MessageHandler : IMessageHandler
    {
        private const byte MESSAGE_SIZE = 8;
        private const byte MESSAGE_TYPE = 2;

        public bool CanHandleI2C(string messageType)
        {
            return false;
        }

        public bool CanHandleUart(byte messageType, byte messageSize)
        {
            if (messageType == MESSAGE_TYPE && messageSize == MESSAGE_SIZE)
            {
                return true;
            }

            return false;
        }

        public byte[] PrepareI2cPackage(JObject message)
        {
            throw new NotImplementedException();
        }

        public object ReadUart(IDataReader reader, byte messageSize)
        {
            var code = reader.ReadUInt32();
            var bits = reader.ReadByte();
            var protocol = reader.ReadByte();

            return new LPD433Message
            {
                Code = code,
                Bits = bits,
                Protocol = protocol
            };
        }

        public Type SupportedMessageType()
        {
            throw new NotImplementedException();
        }
    }
}
