using System;
using System.Collections.Generic;
using Wirehome.Core.Hardware.IR;
using Wirehome.Core.Interface.Messaging;
using Wirehome.Core.Interface.Native;

namespace Wirehome.Extensions.Messaging
{
    public class InfraredMessage : IBinaryMessage
    {
        public static InfraredMessage Empty = new InfraredMessage(0, 0, 0);
        
        public byte System { get; }
        public uint Code { get; }
        public byte Bits { get; }
        public byte Pin { get; }
        public byte Address { get; }
        public byte Repeats { get; } = 1;

        public InfraredMessage(byte system, uint code, byte bits)
        {
            System = system;
            Code = code;
            Bits = bits;
        }

        public InfraredMessage(uint code, byte address, byte pin, byte repeats = 1)
        {
            Code = code;
            Repeats = repeats;
            Address = address;
            Pin = pin;
        }

        public IfraredSystem IfraredSystem => (IfraredSystem)System;
        public MessageType Type() => MessageType.Infrared;
        public override string ToString() => $"System: {IfraredSystem}, Code: {Code}, Bits: {Bits}";
        public bool CanSerialize(string messageType) => messageType == GetType().Name;
        public bool CanDeserialize(byte messageType, byte messageSize) => messageType == (byte)Type() && messageSize == 6;
        public int GetAddress() => Address;

        public byte[] Serialize()
        {
            var package = new List<byte>
            {
                (byte)Type(),
                Repeats,
                System,
                Bits
            };
            package.AddRange(BitConverter.GetBytes(Code));
            return package.ToArray();
        }

        public object Deserialize(IBinaryReader reader, byte? messageSize = null) => new InfraredMessage(reader.ReadByte(), reader.ReadUInt32(), reader.ReadByte());

    }
}