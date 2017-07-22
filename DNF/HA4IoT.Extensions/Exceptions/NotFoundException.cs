using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HA4IoT.Extensions.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException()
        {

        }

        public NotFoundException(string message) : base(message)
        {

        }
    }
}
