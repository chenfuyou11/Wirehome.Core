using System;
using System.Threading.Tasks;

namespace Wirehome.Contracts.Core
{
    public interface INativeUDPSocket : IDisposable
    {
        event Action<string> OnMessageRecived;
        Task BindServiceNameAsync(int port);
        Task SendResponse(string address, int port, byte[] buffer);
    }
}