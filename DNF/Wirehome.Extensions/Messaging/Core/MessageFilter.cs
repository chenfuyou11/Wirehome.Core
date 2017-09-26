using System;

namespace HA4IoT.Extensions.Messaging.Core
{
    public class MessageFilter : IEquatable<MessageFilter>
    {
        public string SimpleFilter { get; set; }
        public bool IsDefault { get; set; }

        public bool Equals(MessageFilter other)
        {
            if (other == null || string.Compare(SimpleFilter, other.SimpleFilter, StringComparison.OrdinalIgnoreCase) != 0 || IsDefault != other.IsDefault) return false;
            
            return true;
        }

        public static implicit operator MessageFilter(string simpleFilter)
        {
            if(simpleFilter.IndexOf("@") > -1)
            {
                return new MessageFilter { IsDefault = true };
            }
            return new MessageFilter { SimpleFilter = simpleFilter };
        }
    }
}
