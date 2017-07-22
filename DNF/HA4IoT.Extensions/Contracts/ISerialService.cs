using HA4IoT.Contracts.Services;

namespace HA4IoT.Extensions
{
    public interface ISerialService : IService
    {
        void RegisterHandler(IMessageHandler handler);
        void Close();
    }
}