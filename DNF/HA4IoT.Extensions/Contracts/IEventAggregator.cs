using System;
using System.Threading;
using System.Threading.Tasks;

namespace HA4IoT.Extensions.Messaging.Core
{
    public interface IEventAggregator
    {
        void ClearSubscriptions();
        void Dispose();
        bool IsSubscribed(Guid token);
        Guid SubscribeForAsyncResult<T>(Func<IMessageEnvelope<T>, Task<object>> action, string context = null);
        void UnSubscribe(Guid token);

        Task<R> PublishWithResultAsync<T, R>
        (
            T message,
            string context = null,
            int millisecondsTimeOut = 2000,
            CancellationToken cancellationToken = default(CancellationToken)
        ) where R : class;
    }
}