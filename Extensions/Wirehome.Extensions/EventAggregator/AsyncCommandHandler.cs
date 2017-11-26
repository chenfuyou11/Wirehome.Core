using System;
using System.Threading.Tasks;

namespace Wirehome.Extensions.Messaging.Core
{
    public sealed class AsyncWithResultCommandHandler : BaseCommandHandler, IAsyncCommandHandler
    {
        public AsyncWithResultCommandHandler(Type type, Guid token, object handler, MessageFilter filter) : base(type, token, handler, filter)
        {
        }

        public async Task<R> HandleAsync<T, R>(IMessageEnvelope<T> message) where R : class
        {
            var handler = Handler as Func<IMessageEnvelope<T>, Task<object>>;
            if(handler == null) throw new InvalidCastException($"Invalid cast from {Handler.GetType()} to Func<IMessageEnvelope<{typeof(T).Name}>, Task<object>>");
            var result = await handler(message).ConfigureAwait(false);
            var typedResult = result as R;
            if (result != null && typedResult == null) throw new InvalidCastException($"Excepted type {typeof(R)} is diffrent that actual {result.GetType()}");
            
            return typedResult;
        }
    }


    public sealed class AsyncCommandHandler : BaseCommandHandler, IAsyncCommandHandler
    {
        public AsyncCommandHandler(Type type, Guid token, object handler, MessageFilter filter) : base(type, token, handler, filter)
        {
        }

        public async Task<R> HandleAsync<T, R>(IMessageEnvelope<T> message) where R : class
        {
            var handler = Handler as Func<IMessageEnvelope<T>, Task>;
            if (handler == null) throw new InvalidCastException($"Invalid cast from {Handler.GetType()} to Func<IMessageEnvelope<{typeof(T).Name}>, Task<object>>");
            await handler(message).ConfigureAwait(false);
 
            return default;
        }
    }



}
