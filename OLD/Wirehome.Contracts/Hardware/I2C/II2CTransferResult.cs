namespace Wirehome.Contracts.Hardware.I2C
{
    public interface II2CTransferResult
    {
        I2CTransferStatus Status { get; }

        int BytesTransferred { get; }
    }
}
