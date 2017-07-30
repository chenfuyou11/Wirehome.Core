using HA4IoT.Extensions.Core;
using Newtonsoft.Json.Linq;
using System;
using Windows.Storage.Streams;

namespace HA4IoT.Extensions.Messaging
{
    public class LPD433Message : IMessage
    {
        private const byte MESSAGE_SIZE = 6;
        private const byte MESSAGE_TYPE = 2;

        public uint Code { get; set; }
        public byte Pin { get; set; } = 1;
        public byte Repeats { get; set; } = 1;

        public byte Bits { get; set; } = 24;
        public byte Protocol { get; set; }

        public DipswitchCode DipswitchCode
        {
            get
            {
                if(Code > 0)
                {
                    return DipswitchCode.ParseCode(Code);
                }

                return null;
            }
        }

        public override string ToString()
        {
            var code = DipswitchCode;
            if(code != null)
            {
                return $"Command: {code.Command}, System: {code.System}, Unit: {code.Unit}";
            }
            
            return $"Code: {Code}, Bits: {Bits}, Protocol: {Protocol}";
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
            var lpd433Message = message.ToObject<LPD433Message>();

            var package = new byte[8];
            package[0] = MESSAGE_TYPE;

            var code = BitConverter.GetBytes(lpd433Message.Code);
            Array.Copy(code, 0, package, 1, 4);

            package[5] = lpd433Message.Bits;
            package[6] = lpd433Message.Repeats;
            package[7] = lpd433Message.Pin;

            return package;
        }

        public object Deserialize(IDataReader reader, byte? messageSize = null)
        {
            var code = reader.ReadUInt32();
            var bits = reader.ReadByte();
            var protocol = reader.ReadByte();

            return new LPD433Message
            {
                Code = code,
                Bits = bits,
                Protocol = protocol
            };
        }
    }
}
