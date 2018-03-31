using Newtonsoft.Json.Linq;
using Wirehome.Core.Native;

namespace Wirehome.ComponentModel.Messaging
{
    public interface IBinaryMessage
    {
        bool CanDeserialize(byte messageType, byte messageSize);

        bool CanSerialize(string messageType);

        object Deserialize(IBinaryReader reader, byte? messageSize = default);

        byte[] Serialize(JObject message);

        MessageType Type();
    }
}