using Windows.Storage.Streams;

namespace HA4IoT.Extensions
{
    public interface IUartMessageHandler
    {
        bool CanHandle(byte messageType, byte messageSize);
        object Handle(DataReader reader);
    }
}