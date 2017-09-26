using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wirehome.Extensions.MessagesModel
{
    public class NotSupportedInCurrentModeError 
    {
        public Header header { get; set; }
        public ErrorPayload payload { get; set; }
    }

    public class ErrorPayload
    {
        public string currentDeviceMode { get; set; }
    }

    
}
