using System;

namespace Wirehome.Contracts.Core
{
    public interface INativeGpio
    {
        event Action ValueChanged;

        void SetDriveMode(NativeGpioPinDriveMode pinMode);
        void Dispose();
        NativeGpioPinValue Read();
        void Write(NativeGpioPinValue pinValue);

        int PinNumber { get; }
    }
}