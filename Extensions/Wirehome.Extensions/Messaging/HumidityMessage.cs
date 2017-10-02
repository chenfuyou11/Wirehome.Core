using Wirehome.Extensions.Contracts;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using Wirehome.Contracts.Core;

namespace Wirehome.Extensions.Messaging
{
    //TODO
    public class HumidityMessage //: IBinaryMessage
    {
        public HumidityMessage()
        {
        }

        public float Humidity { get; set; }
        public byte Pin { get; set; } = 1;

        public MessageType Type()
        {
            return MessageType.Humidity;
        }

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
            var humidity = reader.ReadSingle();

            return new HumidityMessage
            {
                Pin = pin,
                Humidity = humidity
            };
        }

        public string MessageAddress(JObject message)
        {
            throw new NotImplementedException();
        }
    }
}
