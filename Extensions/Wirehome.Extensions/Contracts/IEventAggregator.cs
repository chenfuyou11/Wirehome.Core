using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Wirehome.Extensions.Messaging.Core
{
    // 1. Direct message
    // 2. Publish to all

    public interface IEventAggregator
    {
        void ClearSubscriptions();
        bool IsSubscribed(Guid token);
        void UnSubscribe(Guid token);
        List<Subscription> GetSubscriptors<T>(MessageFilter filter = null);
        SubscriptionToken Subscribe<T>(Action<IMessageEnvelope<T>> action, MessageFilter filter = null);
        SubscriptionToken SubscribeForAsyncResult<T>(Func<IMessageEnvelope<T>, Task<object>> action, MessageFilter filter = null);
        IObservable<IMessageEnvelope<T>> Observe<T>();

        Task<R> SendAsync<T, R>
        (
            T message,
            MessageFilter filter = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default,
            int retryCount = 0
        ) where R : class;

        Task<R> SendWithExpectedResultAsync<T, R>
        (
            T message,
            R expectedResult,
            MessageFilter filter = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default,
            int retryCount = 0
        ) where R : class;

        IObservable<R> SendWithResults<T, R>
        (
            T message,
            MessageFilter filter = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default
        ) where R : class;

       Task SendWithRepublishResult<T, R>
       (
           T message,
           MessageFilter filter = null,
           TimeSpan? timeout = null,
           CancellationToken cancellationToken = default
       ) where R : class;

        Task Publish<T>
        (
            T message,
            MessageFilter filter = null,
            CancellationToken cancellationToken = default
        );
    }
}