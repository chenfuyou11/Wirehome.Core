using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using Windows.Storage.Streams;

namespace HA4IoT.Extensions.Messaging
{
    public class TemperatureMessage : IMessage
    {
        private const byte MESSAGE_SIZE = 5;
        private const byte MESSAGE_TYPE = 1;

        public float Temperature { get; set; }
        public byte Pin { get; set; } = 1;
        
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
            if (messageType == MESSAGE_TYPE && messageSize == MESSAGE_SIZE)
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
                MESSAGE_TYPE,
                currentMessage.Pin
            };

            return package.ToArray();
        }

        public object Deserialize(IDataReader reader, byte? messageSize = null)
        {
            var pin = reader.ReadByte();
            var temp = reader.ReadSingle();

            return new TemperatureMessage
            {
                Pin = pin,
                Temperature = temp
            };
        }
    }
}
