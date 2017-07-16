using System;
using System.Diagnostics;
using Windows.Storage.Streams;

namespace HA4IoT.Extensions
{
    public class InfraredMessageHandler : IUartMessageHandler
    {
        private const byte MESSAGE_SIZE = 6;
        private const byte MESSAGE_TYPE = 1;

        public bool CanHandle(byte messageType, byte messageSize)
        {
            if(messageType == MESSAGE_TYPE && messageSize == MESSAGE_SIZE)
            {
                return true;
            }
            
            return false;
        }

        public object Handle(DataReader reader)
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
    }
}
