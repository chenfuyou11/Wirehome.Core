using System;
using System.Threading;

namespace Wirehome.Extensions.Messaging.Core
{
    public interface IMessageEnvelope<out T>
    {
        CancellationToken CancellationToken { get;  }
        T Message { get; }
        Type ResponseType { get; }
    }
}