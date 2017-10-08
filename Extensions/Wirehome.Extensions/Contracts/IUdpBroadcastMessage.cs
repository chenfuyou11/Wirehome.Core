namespace Wirehome.Extensions.Contracts
{
    public interface IUdpBroadcastMessage
    {
        string MessageAddress();
        byte[] Serialize();
    }
}