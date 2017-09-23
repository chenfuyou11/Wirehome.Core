using HA4IoT.Extensions.Contracts;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace HA4IoT.Extensions.Messaging
{
    public class InfraredMessage : IBinaryMessage
    {
        public byte Repeats { get; set; } = 1;
        public byte System { get; set; }
        public uint Code { get; set; }
        public byte Bits { get; set; }

        public MessageType Type()
        {
            return MessageType.Infrared;
        }

        public override string ToString()
        {
            return $"System: {IfraredSystem}, Code: {Code}, Bits: {Bits}";
        }

        public IfraredSystem IfraredSystem
        {
            get
            {
                return (IfraredSystem)System;
            }
            set
            {
                System = (byte)value;
            }
        }
        
        public bool CanSerialize(string messageType)
        {
            return messageType == GetType().Name;
        }

        public byte[] Serialize(JObject message)
        {
            var infraredMessage = message.ToObject<InfraredMessage>();

            var package = new List<byte>
            {
                (byte)Type(),
                infraredMessage.Repeats,
                infraredMessage.System,
                infraredMessage.Bits
            };
            package.AddRange(BitConverter.GetBytes(infraredMessage.Code));

            return package.ToArray();
        }

        public bool CanDeserialize(byte messageType, byte messageSize)
        {
            if (messageType == (byte)Type() && messageSize == 6)
            {
                return true;
            }

            return false;
        }

        public object Deserialize(IBinaryReader reader, byte? messageSize = null)
        {
            var system = reader.ReadByte();
            var code = reader.ReadUInt32();
            var bits = reader.ReadByte();

            return new InfraredMessage
            {
                System = system,
                Code = code,
                Bits = bits
            };
        }

        public string MessageAddress(JObject message)
        {
            throw new NotImplementedException();
        }
    }



}
