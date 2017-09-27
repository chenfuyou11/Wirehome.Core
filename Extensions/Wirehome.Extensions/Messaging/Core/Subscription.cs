using System;
using System.Threading.Tasks;

namespace Wirehome.Extensions.Messaging.Core
{
    public sealed class Subscription
    {
        internal Guid Token { get; }
        internal Type Type { get; }
        internal MessageFilter Filter { get; }
        private object Handler { get; }

        public Subscription()
        {

        }

        public Subscription(Type type, Guid token, object handler, MessageFilter filter)
        {
            Type = type;
            Token = token;
            Handler = handler;
            //TODO Init
            Filter = filter;
        }

        public bool IsFilterMatch(MessageFilter messageFilter)
        {
            if (messageFilter?.SimpleFilter == "*") return true;

            if (Filter == null &&  messageFilter != null) return false;


            return Filter?.Equals(messageFilter) ?? true;
            
        }

        public async Task<R> HandleAsync<T, R>(IMessageEnvelope<T> message) where R : class
        {
            var handler = Handler as Func<IMessageEnvelope<T>, Task<object>>;
            if(handler == null) throw new InvalidCastException($"Invalid cast from {Handler.GetType()} to Func<IMessageEnvelope<{typeof(T).Name}>, Task<object>>");
            return await handler(message).ConfigureAwait(false) as R;
        }

        public void Handle<T>(IMessageEnvelope<T> message)
        {
            var handler = Handler as Action<IMessageEnvelope<T>>;
            if (handler == null) throw new InvalidCastException($"Invalid cast from {Handler.GetType()} to Action<IMessageEnvelope<{typeof(T).Name}>>");
            handler(message);
        }


    }
}
