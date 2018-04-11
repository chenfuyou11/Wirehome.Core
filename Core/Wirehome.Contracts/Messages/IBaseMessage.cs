using System;
using System.Threading;

namespace Wirehome.Core.Interface.Messaging
{
    public interface IBaseMessage
    {
        string Address { get; set; }
    }
}