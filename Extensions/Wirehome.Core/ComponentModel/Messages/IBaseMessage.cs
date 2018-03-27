using System;
using System.Threading;

namespace Wirehome.ComponentModel
{
    public interface IBaseMessage
    {
        string Address { get; set; }
        Type DefaultService { get; set; }
    }
}