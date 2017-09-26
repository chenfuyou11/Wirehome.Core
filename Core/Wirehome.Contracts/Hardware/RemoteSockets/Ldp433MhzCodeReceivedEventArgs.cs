using System;
using Wirehome.Contracts.Hardware.RemoteSockets.Codes;

namespace Wirehome.Contracts.Hardware.RemoteSockets
{
    public sealed class Ldp433MhzCodeReceivedEventArgs : EventArgs
    {
        public Ldp433MhzCodeReceivedEventArgs(Lpd433MhzCode code)
        {
            Code = code ?? throw new ArgumentNullException(nameof(code));
        }

        public Lpd433MhzCode Code { get; }
    }
}
