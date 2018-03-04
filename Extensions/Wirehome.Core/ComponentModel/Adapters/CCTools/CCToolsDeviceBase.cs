using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using Wirehome.ComponentModel.Adapters.Drivers;
using Wirehome.ComponentModel.Capabilities;
using Wirehome.ComponentModel.Capabilities.Constants;
using Wirehome.ComponentModel.Commands;
using Wirehome.ComponentModel.Commands.Responses;
using Wirehome.ComponentModel.ValueTypes;
using Wirehome.Core;
using Wirehome.Core.Communication.I2C;
using Wirehome.Core.EventAggregator;
using Wirehome.Core.Extensions;

namespace Wirehome.ComponentModel.Adapters
{
    public abstract class CCToolsBaseAdapter : Adapter
    {
        private readonly object _syncRoot = new object();
        protected readonly ILogger _log;
        protected readonly II2CBusService _i2CBusService;
        protected II2CPortExpanderDriver _portExpanderDriver;
        
        private byte[] _committedState;
        private byte[] _state;
        private readonly IEventAggregator _eventAggregator;

        protected CCToolsBaseAdapter(IEventAggregator eventAggregator, II2CBusService i2CBusService, ILogger log)
        {
            _i2CBusService = i2CBusService ?? throw new ArgumentNullException(nameof(i2CBusService));
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
        }

        public override Task Initialize()
        {
            _state = new byte[_portExpanderDriver.StateSize];
            _committedState = new byte[_portExpanderDriver.StateSize];

            _disposables.Add(_eventAggregator.SubscribeForAsyncResult<DeviceCommand>(async messageEnvelope =>
            {
                var message = messageEnvelope.Message;
                if(message.Type == CommandType.QueryCommand)
                {
                    
                }
                else if (message.Type == CommandType.UpdateCommand)
                {
                    var state = message[PowerState.StateName] as StringValue;
                    var pinNumber = message[AdapterProperties.PinNumber] as IntValue;

                    SetPortState(pinNumber.Value, PowerStateValue.ToBinaryState(state), true);
                }
                else if(message.Type == CommandType.DiscoverCapabilities)
                {
                    return new DiscoveryResponse(new PowerState());
                }

                return null;

            }, new DeviceFilter<DeviceCommand>()));

            return Task.CompletedTask;
        }

        public void FetchState()
        {
            lock (_syncRoot)
            {
                var newState = _portExpanderDriver.Read();
                if (newState.SequenceEqual(_state))
                {
                    return;
                }

                var oldState = _state.ToArray();

                Buffer.BlockCopy(newState, 0, _state, 0, newState.Length);
                Buffer.BlockCopy(newState, 0, _committedState, 0, newState.Length);

                var oldStateBits = new BitArray(oldState);
                var newStateBits = new BitArray(newState);

                for (int i = 0; i < oldStateBits.Length; i++)
                {
                    var oldPinState = oldStateBits.Get(i);
                    var newPinState = newStateBits.Get(i);

                    if (oldPinState == newPinState)
                    {
                        return;
                    }

                    var newBinaryState = newPinState ? BinaryState.High : BinaryState.Low;
                    var oldBinaryState = oldPinState ? BinaryState.High : BinaryState.Low;

                    // TODO Fire event
                }
                

                var statesText = BitConverter.ToString(oldState) + "->" + BitConverter.ToString(newState);
                _log.Info("'" + Uid + "' fetched different state (" + statesText + ")");
            }
        }

        protected void SetState(byte[] state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));

            lock (_syncRoot)
            {
                Buffer.BlockCopy(state, 0, _state, 0, state.Length);
            }
        }
        
        protected void CommitChanges(bool force = false)
        {
            lock (_syncRoot)
            {
                if (!force && _state.SequenceEqual(_committedState))
                {
                    return;
                }

                _portExpanderDriver.Write(_state);
                Buffer.BlockCopy(_state, 0,  _committedState, 0, _state.Length);

                _log.Verbose("Board '" + Uid + "' committed state '" + BitConverter.ToString(_state) + "'.");
            }
        }

        internal BinaryState GetPortState(int id)
        {
            lock (_syncRoot)
            {
                return _state.GetBit(id) ? BinaryState.High : BinaryState.Low;
            }
        }

        internal void SetPortState(int pinNumber, BinaryState state, bool commit)
        {
            lock (_syncRoot)
            {
                _state.SetBit(pinNumber, state == BinaryState.High);

                if (commit)
                {
                    CommitChanges();
                }
            }
        }
    }
}
