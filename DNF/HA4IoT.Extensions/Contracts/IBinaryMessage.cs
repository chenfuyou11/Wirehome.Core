using HA4IoT.Extensions.Messaging;
using Newtonsoft.Json.Linq;
using Windows.Storage.Streams;

namespace HA4IoT.Extensions.Contracts
{
    public interface IBinaryMessage
    {
        bool CanDeserialize(byte messageType, byte messageSize);
        bool CanSerialize(string messageType);
        object Deserialize(IDataReader reader, byte? messageSize = default(byte?));
        byte[] Serialize(JObject message);
        MessageType Type();
        string MessageAddress(JObject message);
    }
}