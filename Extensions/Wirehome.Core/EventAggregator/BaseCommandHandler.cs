using System;

namespace Wirehome.Core.EventAggregator
{
    public abstract class BaseCommandHandler
    {
        internal Guid Token { get; }
        internal Type Type { get; }
        internal MessageFilter Filter { get; }
        internal object Handler { get; }

        protected BaseCommandHandler(Type type, Guid token, object handler, MessageFilter filter)
        {
            Type = type;
            Token = token;
            Handler = handler;
            Filter = filter;
        }

        public bool IsFilterMatch(MessageFilter messageFilter)
        {
            if (messageFilter?.SimpleFilter == "*") return true;

            if (Filter == null && messageFilter != null) return false;

            return Filter?.Equals(messageFilter) ?? true;
        }
    }
}
