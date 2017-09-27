using Wirehome.Extensions.Messaging;
using Newtonsoft.Json.Linq;
using Wirehome.Contracts.Core;

namespace Wirehome.Extensions.Contracts
{
    public interface IBinaryMessage
    {
        bool CanDeserialize(byte messageType, byte messageSize);
        bool CanSerialize(string messageType);
        object Deserialize(IBinaryReader reader, byte? messageSize = default(byte?));
        byte[] Serialize(JObject message);
        MessageType Type();
        string MessageAddress(JObject message);
    }
}