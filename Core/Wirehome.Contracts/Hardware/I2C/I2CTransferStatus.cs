namespace Wirehome.Contracts.Hardware.I2C
{
    public enum I2CTransferStatus
    {
        UnknownError,

        FullTransfer,
        PartialTransfer,
        SlaveAddressNotAcknowledged,
        ClockStretchTimeout,
    }
}
