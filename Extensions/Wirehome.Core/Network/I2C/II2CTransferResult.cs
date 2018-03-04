namespace Wirehome.Core.Communication.I2C
{
    public interface II2CTransferResult
    {
        I2CTransferStatus Status { get; }

        int BytesTransferred { get; }
    }
}
