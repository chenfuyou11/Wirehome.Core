using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Collections.Generic;
using Wirehome.Extensions.Exceptions;
using Wirehome.Extensions.Extensions;

namespace Wirehome.Extensions.Messaging.Core
{
    public sealed class EventAggregator : IEventAggregator, IDisposable
    {
        private readonly TimeSpan DEFAULT_TIMEOUT = TimeSpan.FromMilliseconds(2000);
        private readonly Subscriptions _subscriptions = new Subscriptions();
        
        public List<Subscription> GetSubscriptors<T>(MessageFilter filter = null)
        {
            return _subscriptions.GetCurrentSubscriptions(typeof(T), filter);
        }

        public async Task<R> SendAsync<T, R>
        (
            T message,
            MessageFilter filter = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default,
            int retryCount = 0
        ) where R : class
        {
            var localSubscriptions = GetSubscriptors<T>(filter);

            if (localSubscriptions.Count == 0) return default;

            var messageEnvelope = new MessageEnvelope<T>(message, cancellationToken, typeof(R));

            var publishTask = localSubscriptions.Select(x => Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        return await x.HandleAsync<T, R>(messageEnvelope).ConfigureAwait(false);
                    }
                    catch when (retryCount-- > 0) { }
                }
            }));

            return await publishTask.WhenAny<R>(timeout ?? DEFAULT_TIMEOUT, cancellationToken).ConfigureAwait(false);
        }

        public async Task<R> SendWithExpectedResultAsync<T, R>
        (
            T message,
            R expectedResult,
            MessageFilter filter = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default,
            int retryCount = 0
        ) where R : class
        {
            var result = await SendAsync<T, R>(message, filter, timeout, cancellationToken, retryCount).ConfigureAwait(false);
            if(!EqualityComparer<R>.Default.Equals(result, expectedResult))
            {
                throw new WrongResultException(result, expectedResult);
            }
            return result;
        }

        public IObservable<R> SendWithResults<T, R>
        (
            T message,
            MessageFilter filter = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default
        ) where R : class
        {
            var localSubscriptions = GetSubscriptors<T>(filter);

            if (localSubscriptions.Count == 0) return Observable.Empty<R>();

            var messageEnvelope = new MessageEnvelope<T>(message, cancellationToken, typeof(R));

            return localSubscriptions.Select(x => Task.Run(() => x.HandleAsync<T, R>(messageEnvelope)))
                                     .ToObservable()
                                     .SelectMany(x => x)
                                     .Timeout(timeout ?? DEFAULT_TIMEOUT);
        }
        
        public async Task SendWithRepublishResult<T, R>
        (
            T message,
            MessageFilter filter = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default
        ) where R : class
        {
            var localSubscriptions = GetSubscriptors<T>(filter);

            if (localSubscriptions.Count == 0) return;

            var messageEnvelope = new MessageEnvelope<T>(message, cancellationToken, typeof(R));

            var publishTask = localSubscriptions.Select(x => Task.Run(async () =>
            {
                var result = await x.HandleAsync<T, R>(messageEnvelope).ConfigureAwait(false);
                await Publish(result).ConfigureAwait(false);
            }));

            await publishTask.WhenAll(timeout ?? DEFAULT_TIMEOUT, cancellationToken).Unwrap().ConfigureAwait(false);
        }

        public Task Publish<T>
        (
           T message,
           MessageFilter filter = null,
           CancellationToken cancellationToken = default
        )
        {
            var localSubscriptions = GetSubscriptors<T>(filter);
            var messageEnvelope = new MessageEnvelope<T>(message, cancellationToken);

            if (localSubscriptions.Count == 0) return Task.CompletedTask;

            var result = localSubscriptions.Select(x => Task.Run(() =>
            {
                x.Handle<T>(messageEnvelope);
            }));

            return Task.WhenAll(result);
        }

        public SubscriptionToken SubscribeForAsyncResult<T>(Func<IMessageEnvelope<T>, Task<object>> action, MessageFilter filter = null)
        {
            return new SubscriptionToken(_subscriptions.RegisterForAsyncResult(action, filter), this);
        }

        public SubscriptionToken Subscribe<T>(Action<IMessageEnvelope<T>> action, MessageFilter filter = null)
        {
            return new SubscriptionToken(_subscriptions.Register(action, filter), this);
        }
        
        public IObservable<IMessageEnvelope<T>> Observe<T>()
        {
            return Observable.Create<IMessageEnvelope<T>>(x => Subscribe<T>(x.OnNext));
        }
        
        public void UnSubscribe(Guid token)
        {
            _subscriptions.UnRegister(token);
        }

        public bool IsSubscribed(Guid token)
        {
            return _subscriptions.IsRegistered(token);
        }

        public void ClearSubscriptions()
        {
            _subscriptions.Clear();
        }

        public void Dispose()
        {
            ClearSubscriptions();
        }
    }
}