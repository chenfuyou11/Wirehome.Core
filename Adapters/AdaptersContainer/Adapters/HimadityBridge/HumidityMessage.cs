using System.Collections.Generic;
using Wirehome.ComponentModel.ValueTypes;
using Wirehome.Core.Interface.Messaging;
using Wirehome.Core.Interface.Native;

namespace Wirehome.Extensions.Messaging
{
    public class HumidityMessage : IBinaryMessage
    {
        public static HumidityMessage Empty = new HumidityMessage(0, 0.0);

        public DoubleValue Humidity { get; }
        public IntValue Pin { get; }
        public IntValue Address { get; }

        public HumidityMessage(IntValue pin, DoubleValue humidity)
        {
            Pin = pin;
            Humidity = humidity;
        }

        public HumidityMessage(IntValue pin, IntValue address) : this(pin, 0.0)
        {
            Address = address;
        }

        public MessageType Type() => MessageType.Humidity;
        public override string ToString() => $"Humidity {Humidity} on pin {Pin}";
        public bool CanSerialize(string messageType) => messageType == GetType().Name;
        public bool CanDeserialize(byte messageType, byte messageSize) => messageType == (byte)Type() && messageSize == 5;
        public object Deserialize(IBinaryReader reader, byte? messageSize) => new HumidityMessage(reader.ReadByte(), reader.ReadSingle());
        public byte[] Serialize() => new List<byte>  {(byte)Type(), (byte)Pin.Value}.ToArray();
        public int GetAddress() => Address.Value;

    }
}