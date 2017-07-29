using HA4IoT.Contracts.Services;
using HA4IoT.Extensions.Messaging;

namespace HA4IoT.Extensions
{
    public interface ISerialService : IService
    {
        void RegisterHandler(IMessage handler);
        void Close();
    }
}