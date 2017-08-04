using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HA4IoT.Extensions.Messaging
{
    public enum MessageType
    {
        Temperature = 1,
        LPD433 = 2,
        Infrared = 3,
        InfraredRAW = 4,
        Current = 5,
        Humidity = 6,
        Debug = 10,
        Denon = 20
        
    }
}
