using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using Windows.Storage.Streams;

namespace HA4IoT.Extensions.Messaging
{
    public class CurrentMessage : IMessage
    {
        private const byte MESSAGE_SIZE = 2;
        private const byte MESSAGE_TYPE = 5;

        public byte Pin { get; set; }
        public byte State { get; set; }

        public override string ToString()
        {
            return $"New Current state {State} on Pin {Pin}";
        }
        
        public bool CanSerialize(string messageType)
        {
            return messageType == GetType().Name;
        }

        public bool CanDeserialize(byte messageType, byte messageSize)
        {
            if (messageType == MESSAGE_TYPE && messageSize == MESSAGE_SIZE)
            {
                return true;
            }

            return false;
        }

        public byte[] Serialize(JObject message)
        {
            var currentMessage = message.ToObject<CurrentMessage>();

            var package = new List<byte>
            {
                MESSAGE_TYPE,
                currentMessage.Pin
            };
            
            return package.ToArray();
        }

        public object Deserialize(IDataReader reader, byte? messageSize = null)
        {
            var pin = reader.ReadByte();
            var state = reader.ReadByte();

            return new CurrentMessage
            {
                Pin = pin,
                State = state
            };
        }
    }
}
