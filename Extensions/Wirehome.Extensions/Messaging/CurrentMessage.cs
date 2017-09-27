using Wirehome.Extensions.Contracts;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using Wirehome.Contracts.Core;

namespace Wirehome.Extensions.Messaging
{
    public class CurrentMessage : IBinaryMessage
    {
        public byte Pin { get; set; }
        public byte State { get; set; }

        public MessageType Type()
        {
            return MessageType.Current;
        }

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
            if (messageType == (byte)Type() && messageSize == 2)
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
                (byte)Type(),
                currentMessage.Pin
            };
            
            return package.ToArray();
        }

        public object Deserialize(IBinaryReader reader, byte? messageSize = null)
        {
            var pin = reader.ReadByte();
            var state = reader.ReadByte();

            return new CurrentMessage
            {
                Pin = pin,
                State = state
            };
        }

        public string MessageAddress(JObject message)
        {
            throw new NotImplementedException();
        }
    }
}
