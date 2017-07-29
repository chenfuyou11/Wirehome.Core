using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using Windows.Storage.Streams;

namespace HA4IoT.Extensions.Messaging
{
    public class InfraredMessage : IMessage
    {
        private const byte MESSAGE_SIZE = 6;
        private const byte MESSAGE_TYPE = 3;

        public byte Repeats { get; set; } = 1;
        public byte System { get; set; }
        public uint Code { get; set; }
        public byte Bits { get; set; }

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
                MESSAGE_TYPE,
                infraredMessage.Repeats,
                infraredMessage.System,
                infraredMessage.Bits
            };
            package.AddRange(BitConverter.GetBytes(infraredMessage.Code));

            return package.ToArray();
        }

        public bool CanDeserialize(byte messageType, byte messageSize)
        {
            if (messageType == MESSAGE_TYPE && messageSize == MESSAGE_SIZE)
            {
                return true;
            }

            return false;
        }

        public object Deserialize(IDataReader reader, byte? messageSize = null)
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
    }



}
