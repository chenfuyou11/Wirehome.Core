using System.Linq;
using Windows.Storage.Streams;

namespace HA4IoT.Extensions
{
    public class InfraredRawMessageHandler : IUartMessageHandler
    {
        private const byte MESSAGE_TYPE = 3;

        public bool CanHandle(byte messageType, byte messageSize)
        {
            if (messageType == MESSAGE_TYPE)
            {
                return true;
            }

            return false;
        }

        public object Handle(DataReader reader, byte messageSize)
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
    }
}
