namespace Wirehome.ComponentModel.Messaging
{
    public interface IUdpBroadcastMessage
    {
        string MessageAddress();

        byte[] Serialize();
    }
}