using System;
using System.Collections.Generic;
using Wirehome.ComponentModel.ValueTypes;
using Wirehome.Core.Interface.Messaging;
using Wirehome.Core.Interface.Native;

namespace Wirehome.Extensions.Messaging
{
    public class DebugMessage : IBinaryMessage
    {
        public static DebugMessage Empty = new DebugMessage("");
        public StringValue Message { get; }

        public DebugMessage(StringValue message)
        {
            Message = message;
        }


        public MessageType Type() => MessageType.Debug;
        public override string ToString() => $"Debug message {Message}";
        public bool CanSerialize(string messageType) => false;
        public byte[] Serialize() => throw new NotImplementedException();
        public int GetAddress() => 0;
        public bool CanDeserialize(byte messageType, byte messageSize) => messageType == (byte)Type();
        public object Deserialize(IBinaryReader reader, byte? messageSize) => new DebugMessage(reader.ReadString(messageSize.GetValueOrDefault()));
    }
}