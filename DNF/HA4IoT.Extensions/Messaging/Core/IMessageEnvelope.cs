using System.Threading;

namespace HA4IoT.Extensions.Messaging.Core
{
    public interface IMessageEnvelope<out T>
    {
        CancellationToken CancellationToken { get;  }
        T Message { get; }
    }
}