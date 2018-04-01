using Newtonsoft.Json.Linq;
using System;
using Wirehome.ComponentModel.Messaging;
using Wirehome.Core.Hardware.RemoteSockets;
using Wirehome.Core.Native;

namespace Wirehome.Extensions.Messaging
{
    public class RemoteSocketMessage : IBinaryMessage
    {
        public uint Code { get; set; }
        public byte Pin { get; set; } = 1;
        public byte Repeats { get; set; } = 1;
        public byte Bits { get; set; } = 24;
        public byte Protocol { get; set; }

        public MessageType Type()
        {
            return MessageType.LPD433;
        }

        public DipswitchCode DipswitchCode
        {
            get
            {
                if (Code > 0)
                {
                    return DipswitchCode.ParseCode(Code);
                }

                return null;
            }
        }

        public override string ToString()
        {
            var code = DipswitchCode;
            if (code != null)
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
            if (messageType == (byte)Type() && messageSize == 6)
            {
                return true;
            }

            return false;
        }

        public byte[] Serialize()
        {
            var package = new byte[8];
            package[0] = (byte)Type();

            var code = BitConverter.GetBytes(Code);
            Array.Copy(code, 0, package, 1, 4);

            package[5] = Bits;
            package[6] = Repeats;
            package[7] = Pin;

            return package;
        }

        public object Deserialize(IBinaryReader reader, byte? messageSize = null)
        {
            var code = reader.ReadUInt32();
            var bits = reader.ReadByte();
            var protocol = reader.ReadByte();

            return new RemoteSocketMessage
            {
                Code = code,
                Bits = bits,
                Protocol = protocol
            };
        }

        public byte[] Serialize(JObject message)
        {
            throw new NotImplementedException();
        }
    }
}