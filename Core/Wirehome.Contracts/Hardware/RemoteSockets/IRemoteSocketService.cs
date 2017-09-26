using Wirehome.Contracts.Hardware.RemoteSockets.Codes;
using Wirehome.Contracts.Services;

namespace Wirehome.Contracts.Hardware.RemoteSockets
{
    public interface IRemoteSocketService : IService
    {
        IBinaryOutput GetRemoteSocket(string id);
        IBinaryOutput RegisterRemoteSocket(string id, string adapterDeviceId, Lpd433MhzCodePair codePair);

        void SendCode(string adapterDeviceId, Lpd433MhzCode code);
        void RegisterRemoteSockets();

    }
}