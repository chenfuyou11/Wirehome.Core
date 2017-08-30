using System;
using System.Threading;

namespace HA4IoT.Extensions.Messaging.Core
{
    public interface IBaseMessage
    {
        string Address { get; set; }
        Type DefaultService { get; set; }
    }
}