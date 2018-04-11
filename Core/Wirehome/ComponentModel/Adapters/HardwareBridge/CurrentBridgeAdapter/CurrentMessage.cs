using System.Collections.Generic;
using Wirehome.ComponentModel.ValueTypes;
using Wirehome.Core.Interface.Messaging;
using Wirehome.Core.Interface.Native;

namespace Wirehome.Extensions.Messaging
{
    public class CurrentMessage : IBinaryMessage
    {
        public static CurrentMessage Empty = new CurrentMessage(0, 0);

        public IntValue Current { get; }
        public IntValue Pin { get; }
        public IntValue Address { get; }

        public CurrentMessage(IntValue pin, IntValue current)
        {
            Pin = pin;
            Current = current;
        }

        public CurrentMessage(IntValue pin, IntValue address, IntValue current) : this(pin, current)
        {
            Address = address;
        }

        public MessageType Type() => MessageType.Current;
        public override string ToString() => $"Current {Current} on pin {Pin}";
        public bool CanSerialize(string messageType) => messageType == GetType().Name;
        public bool CanDeserialize(byte messageType, byte messageSize) => messageType == (byte)Type() && messageSize == 2;
        public object Deserialize(IBinaryReader reader, byte? messageSize) => new CurrentMessage(reader.ReadByte(), reader.ReadByte());
        public byte[] Serialize() => new List<byte>  {(byte)Type(), (byte)Pin.Value}.ToArray();
        public int GetAddress() => Address.Value;

    }
}