using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
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
