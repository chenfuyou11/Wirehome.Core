using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HA4IoT.Extensions.Messaging.Core
{
    public interface IEventAggregator
    {
        void ClearSubscriptions();
        bool IsSubscribed(Guid token);
        void UnSubscribe(Guid token);
        List<Subscription> GetSubscriptors<T>(MessageFilter filter = null);

        Guid Subscribe<T>(Action<IMessageEnvelope<T>> action, MessageFilter filter = null);
        Guid SubscribeForAsyncResult<T>(Func<IMessageEnvelope<T>, Task<object>> action, MessageFilter filter = null);
        

        Task<R> PublishWithResultAsync<T, R>
        (
            T message,
            MessageFilter filter = null,
            int millisecondsTimeOut = 2000,
            CancellationToken cancellationToken = default(CancellationToken),
            int retryCount = 0
        ) where R : class;

        IObservable<R> PublishWithResults<T, R>
        (
            T message,
            MessageFilter filter = null,
            int millisecondsTimeOut = 2000,
            CancellationToken cancellationToken = default(CancellationToken)
        ) where R : class;

        Task Publish<T>
        (
            T message,
            MessageFilter filter = null,
            CancellationToken cancellationToken = default(CancellationToken)
        );

        Task PublishWithRepublishResult<T, R>
        (
            T message,
            MessageFilter filter = null,
            int millisecondsTimeOut = 2000,
            CancellationToken cancellationToken = default(CancellationToken)
        ) where R : class;
    }
}