using HA4IoT.Extensions.Contracts;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace HA4IoT.Extensions.Messaging
{
    public class TemperatureMessage : IBinaryMessage
    {
        public float Temperature { get; set; }
        public byte Pin { get; set; } = 1;

        public MessageType Type()
        {
            return MessageType.Temperature;
        }

        public override string ToString()
        {
            return $"New temperature {Temperature} on pin {Pin}";
        }
        
        public bool CanSerialize(string messageType)
        {
            return messageType == GetType().Name;
        }

        public bool CanDeserialize(byte messageType, byte messageSize)
        {
            if (messageType == (byte)Type() && messageSize == 5)
            {
                return true;
            }

            return false;
        }

        public byte[] Serialize(JObject message)
        {
            var currentMessage = message.ToObject<TemperatureMessage>();

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
            var temp = reader.ReadSingle();

            return new TemperatureMessage
            {
                Pin = pin,
                Temperature = temp
            };
        }

        public string MessageAddress(JObject message)
        {
            throw new NotImplementedException();
        }
    }
}
