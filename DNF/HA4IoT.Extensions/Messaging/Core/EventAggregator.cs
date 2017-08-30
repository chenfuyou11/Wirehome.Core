using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HA4IoT.Extensions.Extensions;
using System.Reactive.Linq;

namespace HA4IoT.Extensions.Messaging.Core
{
    public sealed class EventAggregator : IEventAggregator
    {
        private readonly Subscriptions _Subscriptions = new Subscriptions();

        public async Task<R> PublishWithResultAsync<T, R>
        (
            T message,
            string context = null,
            int millisecondsTimeOut = 2000,
            CancellationToken cancellationToken = default(CancellationToken)
        ) where R : class
        {
            var localSubscriptions = _Subscriptions.GetCurrentSubscriptions(typeof(T));

            if (localSubscriptions.Count == 0) return default(R);

            var messageEnvelope = new MessageEnvelope<T>(message, cancellationToken);

            var publishTask = localSubscriptions.Select(x => Task.Run(() => x.HandleAsync<T, R>(messageEnvelope)));

            return await publishTask.WhenAny<R>(millisecondsTimeOut, cancellationToken).ConfigureAwait(false);
        }

        public Guid SubscribeForAsyncResult<T>(Func<IMessageEnvelope<T>, Task<object>> action, string context = null)
        {
            return _Subscriptions.Register(action, context);
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

        public void Dispose()
        {
            _Subscriptions.Dispose();
        }

        //public async Task<K> PublishWithResultAsync2<T, K>
        //(
        //    T message,
        //    string context = null, 
        //    int millisecondsTimeOut = 2000, 
        //    CancellationToken cancellationToken = default(CancellationToken)
        //)
        //{
        //    var localSubscriptions = _Subscriptions.GetCurrentSubscriptions(typeof(T));

        //    var taskSource = new TaskCompletionSource<K>();
        //    var resultTask = taskSource.Task;

        //    var subscription = Subscribe<K>(m  =>
        //    {
        //        if(m.Exception != null)
        //        {
        //            taskSource.SetException(m.Exception);
        //        }
        //        else
        //        {
        //            taskSource.SetResult(m.Message);
        //        }

        //        return Task.FromResult(true);
        //    }, context);

        //    try
        //    {
        //        var publishTask = PublishAsync(message, context, cancellationToken);

        //        await(new[] { publishTask, resultTask }).WhenAll(millisecondsTimeOut, cancellationToken);
        //    }
        //    catch (Exception)
        //    {
        //        taskSource.SetCanceled();
        //        throw;
        //    }
        //    finally
        //    {
        //        UnSubscribe(subscription);
        //    }

        //    return resultTask.Result;
        //}

        //public Task PublishResponseAsync<T>(T message, Guid orginalMessageId, Exception exception = null)
        //{
        //    var localSubscriptions = _Subscriptions.GetCurrentSubscriptions(typeof(MessageEnvelope<T>));
        //    var messageid = Guid.NewGuid();
        //    var messageEnvelope = new MessageEnvelope<T>
        //    {
        //        ID = messageid,
        //        Message = message,
        //        Exception = exception,
        //        Context = orginalMessageId.ToString()
        //    };

        //    return localSubscriptions.ForEachAsync(x => { x.Handle(messageEnvelope); });
        //}

        //public Task PublishAsync<T>(T message, string context = null, CancellationToken cancellationToken = default(CancellationToken))
        //{
        //    var localSubscriptions = _Subscriptions.GetCurrentSubscriptions(typeof(T));
        //    var messageid = Guid.NewGuid();
        //    var messageEnvelope = new MessageEnvelope<T>
        //    {
        //        ID = messageid,
        //        Context = context,
        //        Message = message,
        //        CancellationToken = cancellationToken
        //    };

        //    return localSubscriptions.ForEachAsync(x => { x.Handle(messageEnvelope); }, cancellationToken);
        //}
    }
}
