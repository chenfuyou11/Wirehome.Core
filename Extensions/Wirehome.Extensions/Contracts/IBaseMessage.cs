using System;
using System.Threading;

namespace Wirehome.Extensions.Messaging.Core
{
    public interface IBaseMessage
    {
        string Address { get; set; }
        Type DefaultService { get; set; }
    }
}