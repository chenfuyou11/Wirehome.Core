using System;
using System.Collections.Generic;
using System.Text;

namespace Wirehome.Core.EventAggregator
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class MessageFilterAttribute : Attribute
    {
        public MessageFilterAttribute(string simpleFilter)
        {
            SimpleFilter = simpleFilter;
        }

        public string SimpleFilter { get; }

        public MessageFilter ToMessageFilter()
        {
            return new MessageFilter { SimpleFilter = this.SimpleFilter };
        }
    }
}
