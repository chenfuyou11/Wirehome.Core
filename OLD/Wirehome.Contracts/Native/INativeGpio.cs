using System;

namespace Wirehome.Contracts.Core
{
    public interface INativeGpio : IDisposable
    {
        event Action ValueChanged;

        void SetDriveMode(NativeGpioPinDriveMode pinMode);
        NativeGpioPinValue Read();
        void Write(NativeGpioPinValue pinValue);

        int PinNumber { get; }
    }
}