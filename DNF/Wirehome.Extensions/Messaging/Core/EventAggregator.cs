using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HA4IoT.Extensions.Extensions;
using System.Reactive.Linq;
using System.Collections.Generic;
using HA4IoT.Extensions.Exceptions;

namespace HA4IoT.Extensions.Messaging.Core
{
    public sealed class EventAggregator : IEventAggregator
    {
        private readonly Subscriptions _Subscriptions = new Subscriptions();

        public List<Subscription> GetSubscriptors<T>(MessageFilter filter = null)
        {
            return _Subscriptions.GetCurrentSubscriptions(typeof(T), filter);
        }

        public async Task<R> PublishWithResultAsync<T, R>
        (
            T message,
            MessageFilter filter = null,
            int millisecondsTimeOut = 2000,
            CancellationToken cancellationToken = default,
            int retryCount = 0
        ) where R : class
        {
            var localSubscriptions = GetSubscriptors<T>(filter);

            if (localSubscriptions.Count == 0) return default(R);

            var messageEnvelope = new MessageEnvelope<T>(message, cancellationToken);

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

            return await publishTask.WhenAny<R>(millisecondsTimeOut, cancellationToken).ConfigureAwait(false);
        }

        public async Task<R> PublishWithExpectedResultAsync<T, R>
        (
            T message,
            R expectedResult,
            MessageFilter filter = null,
            int millisecondsTimeOut = 2000,
            CancellationToken cancellationToken = default,
            int retryCount = 0
        ) where R : class
        {
            var result = await PublishWithResultAsync<T, R>(message, filter, millisecondsTimeOut, cancellationToken, retryCount).ConfigureAwait(false);
            if(!EqualityComparer<R>.Default.Equals(result, expectedResult))
            {
                throw new WrongResultException(result, expectedResult);
            }
            return result;
        }

        public IObservable<R> PublishWithResults<T, R>
        (
            T message,
            MessageFilter filter = null,
            int millisecondsTimeOut = 2000,
            CancellationToken cancellationToken = default
        ) where R : class
        {
            var localSubscriptions = GetSubscriptors<T>(filter);

            if (localSubscriptions.Count == 0) return Observable.Empty<R>();

            var messageEnvelope = new MessageEnvelope<T>(message, cancellationToken);

            return localSubscriptions.Select(x => Task.Run(() => x.HandleAsync<T, R>(messageEnvelope)))
                                     .ToObservable()
                                     .SelectMany(x => x)
                                     .Timeout(TimeSpan.FromMilliseconds(millisecondsTimeOut));
        }

        public Task Publish<T>
        (
            T message,
            MessageFilter filter = null,
            CancellationToken cancellationToken = default
        )
        {
            var localSubscriptions = GetSubscriptors<T>(filter);

            if (localSubscriptions.Count == 0) return Task.Delay(0);

            var messageEnvelope = new MessageEnvelope<T>(message, cancellationToken);

            var result = localSubscriptions.Select(x => Task.Run(() =>
            {
                x.Handle<T>(messageEnvelope);
            }));

            return Task.WhenAll(result);
        }

        public async Task PublishWithRepublishResult<T, R>
        (
            T message,
            MessageFilter filter = null,
            int millisecondsTimeOut = 2000,
            CancellationToken cancellationToken = default
        ) where R : class
        {
            var localSubscriptions = GetSubscriptors<T>(filter);

            if (localSubscriptions.Count == 0) return;

            var messageEnvelope = new MessageEnvelope<T>(message, cancellationToken);

            var publishTask = localSubscriptions.Select(x => Task.Run(async () =>
            {
                var result = await x.HandleAsync<T, R>(messageEnvelope).ConfigureAwait(false);
                await Publish(result).ConfigureAwait(false);
            }));

            await publishTask.WhenAll(millisecondsTimeOut, cancellationToken).ConfigureAwait(false);
        }

        public Guid SubscribeForAsyncResult<T>(Func<IMessageEnvelope<T>, Task<object>> action, MessageFilter filter = null)
        {
            return _Subscriptions.RegisterForAsyncResult(action, filter);
        }

        public Guid Subscribe<T>(Action<IMessageEnvelope<T>> action, MessageFilter filter = null)
        {
            return _Subscriptions.Register(action, filter);
        }

        public void UnSubscribe(Guid token)
        {
            _Subscriptions.UnRegister(token);
        }

        public bool IsSubscribed(Guid token)
        {
            return _Subscriptions.IsRegistered(token);
        }

        public void ClearSubscriptions()
        {
            _Subscriptions.Clear();
        }
        
    }
}