using System.Collections.Generic;
using Wirehome.ComponentModel.ValueTypes;
using Wirehome.Core.Interface.Messaging;
using Wirehome.Core.Interface.Native;

namespace Wirehome.Extensions.Messaging
{
    public class TemperatureMessage : IBinaryMessage
    {
        public static TemperatureMessage Empty = new TemperatureMessage(0, 0.0);

        public DoubleValue Temperature { get; }
        public IntValue Pin { get; }
        public IntValue Address { get; }

        public TemperatureMessage(IntValue pin, DoubleValue temperature)
        {
            Pin = pin;
            Temperature = temperature;
        }

        public TemperatureMessage(IntValue pin, IntValue address) : this(pin, 0.0)
        {
            Address = address;
        }

        public MessageType Type() => MessageType.Temperature;
        public override string ToString() => $"Temperature {Temperature} on pin {Pin}";
        public bool CanSerialize(string messageType) => messageType == GetType().Name;
        public bool CanDeserialize(byte messageType, byte messageSize) => messageType == (byte)Type() && messageSize == 5;
        public object Deserialize(IBinaryReader reader, byte? messageSize) => new TemperatureMessage(reader.ReadByte(), reader.ReadSingle());
        public byte[] Serialize() => new List<byte>  {(byte)Type(), (byte)Pin.Value}.ToArray();
        public int GetAddress() => Address.Value;

    }
}