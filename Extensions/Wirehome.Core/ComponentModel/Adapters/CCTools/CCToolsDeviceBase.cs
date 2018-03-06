using Quartz;
using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Wirehome.ComponentModel.Adapters.Drivers;
using Wirehome.ComponentModel.Capabilities;
using Wirehome.ComponentModel.Capabilities.Constants;
using Wirehome.ComponentModel.Commands;
using Wirehome.ComponentModel.Commands.Responses;
using Wirehome.ComponentModel.Events;
using Wirehome.ComponentModel.ValueTypes;
using Wirehome.Core;
using Wirehome.Core.Communication.I2C;
using Wirehome.Core.EventAggregator;
using Wirehome.Core.Extensions;
using Wirehome.Core.Services.Quartz;

namespace Wirehome.ComponentModel.Adapters
{
    public abstract class CCToolsBaseAdapter : Adapter
    {
        protected readonly ILogger _log;
        protected readonly II2CBusService _i2CBusService;
        protected readonly IEventAggregator _eventAggregator;
        protected readonly IScheduler _scheduler;
        private readonly object _syncRoot = new object();
        private int _poolDurationWarning;
        
        protected II2CPortExpanderDriver _portExpanderDriver;
        private byte[] _committedState;
        private byte[] _state;
        
        protected CCToolsBaseAdapter(IAdapterServiceFactory adapterServiceFactory)
        {
            _i2CBusService = adapterServiceFactory.GetI2CService();
            _log = adapterServiceFactory.GetLogger();
            _eventAggregator = adapterServiceFactory.GetEventAggregator();
            _scheduler = adapterServiceFactory.GetScheduler();
        }

        public override async Task Initialize()
        {
            var poolInterval = (IntValue)this[AdapterProperties.PoolInterval];
            _poolDurationWarning = (IntValue)this[AdapterProperties.PollDurationWarningThreshold];

            _state = new byte[_portExpanderDriver.StateSize];
            _committedState = new byte[_portExpanderDriver.StateSize];

            await _scheduler.ScheduleIntervalWithContext<CCToolsSchedulerJob, CCToolsBaseAdapter>(TimeSpan.FromMilliseconds(poolInterval), this, _disposables.Token).ConfigureAwait(false);

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

                    //TODO read source and invoke event change with this source to distinct with change outside program code
                    //message[CommandProperties.CommandSource]

                    SetPortState(pinNumber.Value, PowerStateValue.ToBinaryState(state), true);
                }
                else if(message.Type == CommandType.DiscoverCapabilities)
                {
                    return new DiscoveryResponse(new PowerState());
                }

                return null;

            }, new DeviceFilter<DeviceCommand>()));
        }

        public void FetchState()
        {
            var stopwatch = Stopwatch.StartNew();
            FetchStateCore();
            stopwatch.Stop();

            if (stopwatch.ElapsedMilliseconds > _poolDurationWarning)
            {
                _log.Warning($"Polling device '{Uid}' took {stopwatch.ElapsedMilliseconds} ms.");
            }
        }

        public void FetchStateCore()
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
                    
                    var properyChangeEvent = new PropertyChangedEvent(Uid, PowerState.StateName, new BooleanValue(oldPinState), new BooleanValue(newPinState));
                    properyChangeEvent[PropertyChangedEventProperty.PinNumber] = (IntValue)i;

                    _eventAggregator.Publish(properyChangeEvent);
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
