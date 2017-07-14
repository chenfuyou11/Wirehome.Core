using Windows.Storage.Streams;

namespace HA4IoT.Extensions
{
    public class InfraredMessageHandler : IInfraredMessageHandler
    {
        private const byte IfraredMessageSize = 20;

        public bool CanHandle(byte messageType, byte messageSize)
        {
            if(messageType == 0 && messageSize == IfraredMessageSize)
            {
                return true;
            }
            
            return false;
        }

        public object Handle(DataReader reader)
        {
            return new InfraredMessage
            {
                System = reader.ReadByte(),
                Code = reader.ReadUInt32(),
                Bits = reader.ReadByte()
            };
        }
    }
}
