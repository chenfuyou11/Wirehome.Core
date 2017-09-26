using System;
using Wirehome.Contracts.Hardware;
using System.Linq;
using Wirehome.Contracts.Components.States;
using System.Threading.Tasks;
using Wirehome.Contracts.Components.Adapters;
using Wirehome.Contracts.Scheduling;
using Wirehome.Scheduling;
using Wirehome.Extensions.Contracts;

namespace Wirehome.Extensions.Core
{

    public class MonostableBinaryOutputAdapter : IBinaryOutputAdapter, IMonostableLampAdapter
    {
        private readonly IBinaryOutput _output;
        private readonly IBinaryInput _input;
        private readonly int ON_TIME = 300;
        private bool _ControledStateChange = false;

        public event Action<PowerStateValue> StateChanged;

        public bool SupportsColor => false;

        public int ColorResolutionBits => 0;

        public MonostableBinaryOutputAdapter(IBinaryOutput output, IBinaryInput input)
        {
            _output = output ?? throw new ArgumentNullException(nameof(output));
            _input = input ?? throw new ArgumentNullException(nameof(input));

            _input.StateChanged += Input_StateChanged;
        }

        public Task SetState(AdapterPowerState powerState, AdapterColor color, params IHardwareParameter[] hardwareParameters)
        {
            if (color != null)
            {
                throw new InvalidOperationException("Color is not supported.");
            }

            SetState(powerState, hardwareParameters);

            return Task.FromResult(0);
        }

        public Task SetState(AdapterPowerState powerState, params IHardwareParameter[] parameters)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            var commit = !parameters.Any(p => p is IsPartOfPartialUpdateParameter);
            lock (_output)
            {
                var inputState = _input.Read();

                if 
                (
                      (inputState == BinaryState.Low && powerState == AdapterPowerState.Off)
                   || (inputState == BinaryState.High && powerState == AdapterPowerState.On)
                )
                {
                    return Task.FromResult(0);
                }

                _ControledStateChange = true;

                _output.Write(BinaryState.High, commit ? WriteBinaryStateMode.Commit : WriteBinaryStateMode.NoCommit);

                ScheduledAction.Schedule(TimeSpan.FromMilliseconds(ON_TIME), () =>
                {
                    _output.Write(BinaryState.Low, commit ? WriteBinaryStateMode.Commit : WriteBinaryStateMode.NoCommit);

                    _ControledStateChange = false;
                });
            }

            return Task.FromResult(0);
        }

        private void Input_StateChanged(Object sender, BinaryStateChangedEventArgs e)
        {
            if(!_ControledStateChange)
            {
                if(e.NewState == BinaryState.High)
                {
                    StateChanged?.Invoke(PowerStateValue.On);
                }
                else
                {
                    StateChanged?.Invoke(PowerStateValue.Off);
                }
               
            }
        }

        
    }
}