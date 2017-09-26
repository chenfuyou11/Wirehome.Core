using System;
using Wirehome.Contracts.Hardware;
using Wirehome.Contracts.Hardware.RemoteSockets.Adapters;
using Wirehome.Contracts.Hardware.RemoteSockets.Codes;

namespace Wirehome.Hardware.RemoteSockets
{
    public sealed class RemoteSocketOutputPort : IBinaryOutput
    {
        private readonly object _syncRoot = new object();
        private readonly Lpd433MhzCodePair _codePair;
        private readonly ILdp433MhzBridgeAdapter _adapter;
        private BinaryState _state;

        public RemoteSocketOutputPort(Lpd433MhzCodePair codePair, ILdp433MhzBridgeAdapter adapter)
        {
            _codePair = codePair ?? throw new ArgumentNullException(nameof(codePair));
            _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
        }

        public event EventHandler<BinaryStateChangedEventArgs> StateChanged;

        public BinaryState Read()
        {
            return _state;
        }

        public void Write(BinaryState state, WriteBinaryStateMode mode = WriteBinaryStateMode.Commit)
        {
            if (mode != WriteBinaryStateMode.Commit)
            {
                return;
            }

            lock (_syncRoot)
            {
                var oldState = state;
                _state = state;

                StateChanged?.Invoke(this, new BinaryStateChangedEventArgs(oldState, state));
            }

            if (state == BinaryState.High)
            {
                _adapter.SendCode(_codePair.OnCode);
            }
            else if (state == BinaryState.Low)
            {
                _adapter.SendCode(_codePair.OffCode);
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }
}
