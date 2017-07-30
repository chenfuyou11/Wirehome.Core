using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using Windows.Storage.Streams;

namespace HA4IoT.Extensions.Messaging
{
    public class HumidityMessage : IMessage
    {
        private const byte MESSAGE_SIZE = 5;
        private const byte MESSAGE_TYPE = 6;

        public float Humidity { get; set; }
        public byte Pin { get; set; } = 1;
        
        public override string ToString()
        {
            return $"New humidity {Humidity} on pin {Pin}";
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
            var humidity = reader.ReadSingle();

            return new HumidityMessage
            {
                Pin = pin,
                Humidity = humidity
            };
        }
    }
}
