using Windows.Storage.Streams;

namespace HA4IoT.Extensions
{
    public class LPD433MessageHandler : IUartMessageHandler
    {
        private const byte MESSAGE_SIZE = 8;
        private const byte MESSAGE_TYPE = 2;

        public bool CanHandle(byte messageType, byte messageSize)
        {
            if (messageType == MESSAGE_TYPE && messageSize == MESSAGE_SIZE)
            {
                return true;
            }

            return false;
        }

        public object Handle(DataReader reader, byte messageSize)
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
    }
}
