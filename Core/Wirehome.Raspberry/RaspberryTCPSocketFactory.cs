using Windows.Networking.Sockets;
using Wirehome.Contracts.Core;

namespace Wirehome.Raspberry
{
    public class RaspberryTCPSocketFactory : INativeTCPSocketFactory
    {
        public INativeTCPSocket Create()
        {
            return new RaspberryTCPSocket(new StreamSocket());
        }
    }
}
