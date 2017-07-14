using Windows.Storage.Streams;

namespace HA4IoT.Extensions
{
    public interface IInfraredMessageHandler
    {
        bool CanHandle(byte messageType, byte messageSize);
        object Handle(DataReader reader);
    }
}