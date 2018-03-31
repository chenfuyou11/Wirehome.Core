using System;
using System.Threading;

namespace Wirehome.ComponentModel.Messaging
{
    public interface IBaseMessage
    {
        string Address { get; set; }
        Type DefaultService { get; set; }
    }
}