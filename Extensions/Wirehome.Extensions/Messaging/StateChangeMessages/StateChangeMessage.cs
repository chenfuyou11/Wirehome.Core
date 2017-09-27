using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wirehome.Extensions.Messaging.StateChangeMessages
{
    public class StateChangeMessage<T>
    {
        public T OldValue { get; set; }
        public T NewValue { get; set; }
        public DateTime ChangeDate { get; set; }
        public string DeviceID { get; set; }

        public StateChangeMessage(string deviceID, T oldValue, T newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
            DeviceID = deviceID;
            ChangeDate = DateTime.Now;
        }
    }
}
