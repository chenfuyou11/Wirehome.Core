using System;
using Wirehome.Core.Hardware.RemoteSockets;
using Wirehome.Core.Interface.Messaging;
using Wirehome.Core.Interface.Native;

namespace Wirehome.Extensions.Messaging
{
    public class RemoteSocketMessage : IBinaryMessage
    {
        public static RemoteSocketMessage Empty = new RemoteSocketMessage(0, 0, 0);

        public uint Code { get;  }
        public byte Bits { get; } = 24;
        public byte Protocol { get; } = 1;
        public byte Pin { get; }
        public byte Repeats { get; } = 1;
        public byte Address { get; }

        public RemoteSocketMessage(uint code, byte bits, byte protocol)
        {
            Code = code;
            Bits = bits;
            Protocol = protocol;
        }

        public RemoteSocketMessage(DipswitchCode code, byte address, byte pin, byte repeats = 1)
        {
            Code = code.Code;
            Repeats = repeats;
            Address = address;
            Pin = pin;
        }

        private DipswitchCode DipswitchCode => DipswitchCode.ParseCode(Code);

        public MessageType Type() => MessageType.RemoteSocket;
        public override string ToString()
        {
            var code = DipswitchCode;
            if (code != null)
            {
                return $"Command: {code.Command}, System: {code.System}, Unit: {code.Unit}";
            }

            return $"Code: {Code}, Bits: {Bits}, Protocol: {Protocol}";
        }
        public bool CanSerialize(string messageType) => messageType == GetType().Name;
        public bool CanDeserialize(byte messageType, byte messageSize) => messageType == (byte)Type() && messageSize == 6;
        public int GetAddress() => Address;

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

            return new RemoteSocketMessage(code, bits, protocol);
        }
    }
}