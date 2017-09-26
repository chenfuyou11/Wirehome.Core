using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HA4IoT.Extensions.MessagesModel
{
    public class TurnRequest
    {
        public string ComponentID { get; set; }
        public string DeviceID { get; set; }
        public string MessageID { get; set; }
        public string Command { get; set; }
        public string Percentage { get; set; }
        public string IncrementValue { get; set; }
        public string DecrementValue { get; set; }
    }
}
