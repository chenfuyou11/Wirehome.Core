using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.Storage.Streams;

namespace HA4IoT.Extensions.Messaging
{
    public class InfraredMessageHandler : IMessageHandler
    {
        private const byte MESSAGE_SIZE = 6;
        private const byte MESSAGE_TYPE = 3;

        public bool CanHandleUart(byte messageType, byte messageSize)
        {
            if(messageType == MESSAGE_TYPE && messageSize == MESSAGE_SIZE)
            {
                return true;
            }
            
            return false;
        }

        public bool CanHandleI2C(string messageType)
        {
            if(messageType == typeof(InfraredMessage).Name)
            {
                return true;
            }

            return false;
        }

        public byte[] PrepareI2cPackage(JObject message)
        {
            var infraredMessage = message.ToObject<InfraredMessage>();

            var package = new List<byte>
            {
                MESSAGE_TYPE,
                infraredMessage.Repeats,
                infraredMessage.System,
                infraredMessage.Bits
            };
            package.AddRange(BitConverter.GetBytes(infraredMessage.Code));

            return package.ToArray();
        }

        public object ReadUart(IDataReader reader, byte messageSize)
        {
            var system = reader.ReadByte();
            var code = reader.ReadUInt32();
            var bits = reader.ReadByte();

            return new InfraredMessage
            {
                System = system,
                Code = code,
                Bits = bits
            };
        }

        public Type SupportedMessageType()
        {
            return typeof(InfraredMessage);
        }
    }
}
