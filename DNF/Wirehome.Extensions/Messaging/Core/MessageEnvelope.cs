using System;
using System.Threading;

namespace HA4IoT.Extensions.Messaging.Core
{
    public class MessageEnvelope<T> : IMessageEnvelope<T>
    {
        public MessageEnvelope(T message, CancellationToken token)
        {
            Message = message;
            CancellationToken = token;
        }

        public T Message { get;  }
        public CancellationToken CancellationToken { get; }
    }
}
