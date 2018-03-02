using System;

namespace Wirehome.Core.EventAggregator
{
    public abstract class BaseCommandHandler
    {
        internal Guid Token { get; }
        internal Type MessageType { get; }
        internal MessageFilter SubscriptionFilter { get; }
        internal object Handler { get; }

        protected BaseCommandHandler(Type type, Guid token, object handler, MessageFilter filter)
        {
            MessageType = type;
            Token = token;
            Handler = handler;
            SubscriptionFilter = filter;
        }

        public bool IsFilterMatch(MessageFilter messageFilter, object message)
        {
            if (messageFilter?.SimpleFilter == "*") return true;

            if (SubscriptionFilter == null && messageFilter != null) return false;

            return SubscriptionFilter?.EvaluateFilter(messageFilter, message) ?? true;
        }
    }
}
