using System;
using Wirehome.Contracts.Devices;
using Wirehome.Contracts.Hardware.RemoteSockets.Codes;

namespace Wirehome.Contracts.Hardware.RemoteSockets.Adapters
{
    public interface ILdp433MhzBridgeAdapter : IDevice
    {
        event EventHandler<Ldp433MhzCodeReceivedEventArgs> CodeReceived;

        void SendCode(Lpd433MhzCode code);
    }
}
