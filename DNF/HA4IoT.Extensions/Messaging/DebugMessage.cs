using System;
using Newtonsoft.Json.Linq;
using Windows.Storage.Streams;

namespace HA4IoT.Extensions.Messaging
{
    public class DebugMessage : IMessage
    {
        private const byte MESSAGE_TYPE = 10;

        public string Message { get; set; }
        
        public bool CanDeserialize(byte messageType, byte messageSize)
        {
            if (messageType == MESSAGE_TYPE)
            {
                return true;
            }

            return false;
        }

        public bool CanSerialize(string messageType)
        {
            return false;
        }

        public object Deserialize(IDataReader reader, byte? messageSize)
        {
            var message = reader.ReadString(messageSize.GetValueOrDefault());

            return new DebugMessage
            {
                Message = message
            };
        }

        public byte[] Serialize(JObject message)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return $"Debug message: {Message}";
        }
    }
}
