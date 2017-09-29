using System;
using System.Threading.Tasks;

namespace Wirehome.Contracts.Core
{
    public interface INativeTCPSocket : IDisposable
    {
        Task ConnectAsync(string hostName, int port, int timeout);
        Task SendDataAsync(byte[] data, int timeout, bool autoflush);
        Task ReadDataAsync(byte[] data, int timeout);
        Task<string> ReadLineAsync();
    }
}