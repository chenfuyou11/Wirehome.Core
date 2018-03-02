using System;
using Wirehome.Core.Extensions;

namespace Wirehome.Core.EventAggregator
{
    public class MessageFilter
    {
        public string SimpleFilter { get; set; }
        public virtual string GetCustomFilter(object message) => string.Empty;
        
        public bool EvaluateFilter(MessageFilter other, object message)
        {
            if (other == null || SimpleFilter.Compare(other.SimpleFilter) != 0) return false;
            if (GetCustomFilter(message).Compare(GetCustomFilter(message)) != 0) return false;
            return true;
        }

        public static implicit operator MessageFilter(string simpleFilter) => new MessageFilter { SimpleFilter = simpleFilter };
    }
    
}
