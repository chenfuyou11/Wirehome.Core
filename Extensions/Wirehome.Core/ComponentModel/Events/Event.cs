using Wirehome.Core.Extensions;

namespace Wirehome.ComponentModel.Events
{
    public class Event : BaseObject, System.IEquatable<Event>
    {
        public Event()
        {
            SupressPropertyChangeEvent = true;
        }

        public bool Equals(Event other)
        {
            if (other == null || Type.Compare(other.Type) != 0 || !Properties.LeftEqual(other.Properties)) return false;
            
            return true;
        }
    }
}
