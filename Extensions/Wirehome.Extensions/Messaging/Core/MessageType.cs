namespace Wirehome.Extensions.Messaging
{
    public enum MessageType
    {
        Temperature = 1,
        LPD433 = 2,
        Infrared = 3,
        InfraredRAW = 4,
        Current = 5,
        Humidity = 6,
        Debug = 10,
        TCP = 20,
        UDP = 30
    }
}
