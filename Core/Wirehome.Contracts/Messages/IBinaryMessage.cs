
using Wirehome.Core.Interface.Native;

namespace Wirehome.Core.Interface.Messaging
{
    public interface IBinaryMessage
    {
        bool CanDeserialize(byte messageType, byte messageSize);

        bool CanSerialize(string messageType);

        object Deserialize(IBinaryReader reader, byte? messageSize = 0);

        byte[] Serialize();

        MessageType Type();

        int GetAddress();
    }
}