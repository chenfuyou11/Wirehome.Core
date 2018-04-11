using System;

namespace Wirehome.Contracts.Hardware
{
    public interface IBinaryInput
    {
        event EventHandler<BinaryStateChangedEventArgs> StateChanged;

        BinaryState Read();
    }
}