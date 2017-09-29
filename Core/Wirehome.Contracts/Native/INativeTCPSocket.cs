using System;
using System.Threading.Tasks;

namespace Wirehome.Contracts.Core
{
    public interface INativeTCPSocket
    {
        Task ConnectAsync(string hostName, int port, int timeout);
        Task SendDataAsync(byte[] data, int timeout);
        Task ReadDataAsync(byte[] data, int timeout);
        void Dispose();
    }
}