using System;
using System.Threading.Tasks;

namespace HA4IoT.Extensions.Messaging.Core
{
    public sealed class Subscription
    {
        internal Guid Token { get; }
        internal Type Type { get; }
        internal string Context { get; }
        private object Handler { get; }

        public Subscription()
        {

        }

        public Subscription(Type type, Guid token, object handler, string context)
        {
            Type = type;
            Token = token;
            Handler = handler;
            Context = context;
        }

        public async Task<R> HandleAsync<T, R>(IMessageEnvelope<T> message) where R : class
        {
            var handler = Handler as Func<IMessageEnvelope<T>, Task<object>>;
            if(handler == null) throw new InvalidCastException($"Invalid cast from {Handler.GetType()} to Func<IMessageEnvelope<{typeof(T).Name}>, Task<object>>");
            return await handler(message).ConfigureAwait(false) as R;
        }
        

    }
}
