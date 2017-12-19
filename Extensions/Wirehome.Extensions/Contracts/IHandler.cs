namespace Wirehome.Extensions.Messaging.Core
{
    public interface IHandler<T>
    {
        void Handle(IMessageEnvelope<T> message);
    }
}