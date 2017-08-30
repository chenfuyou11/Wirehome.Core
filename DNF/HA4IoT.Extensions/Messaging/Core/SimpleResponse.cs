using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HA4IoT.Extensions.Messaging.Core
{
    public class SimpleResponse
    {
        public bool Result { get; set; }

        public SimpleResponse(bool result)
        {
            Result = result;
        }
    }
}
