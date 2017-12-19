using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Wirehome.Extensions.Core.Policies;

namespace Wirehome.Extensions.Messaging.Core
{
    // 1. Direct message
    // 2. Publish to all
    // 3. Filter xxx.yyy.zz
    // 4. Filter key-value

    public interface IEventAggregator
    {

        void ClearSubscriptions();
        bool IsSubscribed(Guid token);
        void UnSubscribe(Guid token);
        List<BaseCommandHandler> GetSubscriptors<T>(MessageFilter filter = null);

        SubscriptionToken Subscribe<T>(Action<IMessageEnvelope<T>> action, MessageFilter filter = null);
        SubscriptionToken Subscribe(Type messageType, object action, MessageFilter filter = null);

        SubscriptionToken SubscribeAsync<T>(Func<IMessageEnvelope<T>, Task> action, MessageFilter filter = null);
        SubscriptionToken SubscribeForAsyncResult<T>(Func<IMessageEnvelope<T>, Task<object>> action, MessageFilter filter = null);

        IObservable<IMessageEnvelope<T>> Observe<T>();
        Func<BehaviorChain> DefaultBehavior { get; set; }

        Task<R> QueryAsync<T, R>
        (
            T message,
            MessageFilter filter = null,
            CancellationToken cancellationToken = default,
            BehaviorChain behaviors = null
        ) where R : class;
        
        Task<R> QueryWithResultCheckAsync<T, R>
        (
            T message,
            R expectedResult,
            MessageFilter filter = null,
            CancellationToken cancellationToken = default,
            BehaviorChain behaviors = null
        ) where R : class;

        IObservable<R> QueryWithResults<T, R>
        (
            T message,
            MessageFilter filter = null,
            CancellationToken cancellationToken = default,
            BehaviorChain behaviors = null
        ) where R : class;

       Task QueryWithRepublishResult<T, R>
       (
           T message,
           MessageFilter filter = null,
           CancellationToken cancellationToken = default,
           BehaviorChain behaviors = null
       ) where R : class;

        Task Publish<T>
        (
            T message,
            MessageFilter filter = null,
            CancellationToken cancellationToken = default,
            BehaviorChain behaviors = null
        );
    }
}