using System;
using System.Threading.Tasks;
using Wirehome.ComponentModel.Capabilities;
using Wirehome.ComponentModel.Commands;
using Wirehome.ComponentModel.Commands.Responses;
using Wirehome.ComponentModel.ValueTypes;
using Wirehome.Core.Extensions;

namespace Wirehome.ComponentModel.Adapters.Samsung
{
    public class SamsungAdapter : Adapter
    {
        private string _hostname;

        private BooleanValue _powerState;
        private DoubleValue _volume;
        private BooleanValue _mute;
        private StringValue _input;

        public SamsungAdapter(IAdapterServiceFactory adapterServiceFactory) : base(adapterServiceFactory)
        {
        }

        public override async Task Initialize()
        {
            base.Initialize();

            _hostname = Properties[AdapterProperties.Hostname].Value.ToStringValue();
        }

        protected DiscoveryResponse DiscoverCapabilitiesHandler(Command message)
        {
            //TODO Add read only state
            return new DiscoveryResponse(RequierdProperties(), new PowerState(),
                                                               new VolumeState(),
                                                               new MuteState(),
                                                               new InputSourceState()
                                          );
        }

        protected async Task TurnOnCommandHandler(Command message)
        {
            //TODO ADD infrared message
        }

        protected async Task TurnOffCommandHandler(Command message)
        {
            await _eventAggregator.QueryAsync<SamsungControlMessage, string>(new SamsungControlMessage
            {
                Address = _hostname,
                Code = "KEY_POWEROFF"
            });
            _powerState = await UpdateState(PowerState.StateName, _powerState, new BooleanValue(false));
        }

        protected async Task VolumeUpCommandHandler(Command command)
        {
            await _eventAggregator.QueryAsync<SamsungControlMessage, string>(new SamsungControlMessage
            {
                Address = _hostname,
                Code = "KEY_VOLUP"
            });
        }

        protected async Task VolumeDownCommandHandler(Command command)
        {
            await _eventAggregator.QueryAsync<SamsungControlMessage, string>(new SamsungControlMessage
            {
                Address = _hostname,
                Code = "KEY_VOLDOWN"
            });
        }

        protected async Task MuteCommandHandler(Command message)
        {
            await _eventAggregator.QueryAsync<SamsungControlMessage, string>(new SamsungControlMessage
            {
                Address = _hostname,
                Code = "KEY_MUTE"
            });

            _mute = await UpdateState(MuteState.StateName, _mute, new BooleanValue(!_mute));
        }

        protected async Task SelectInputCommandHandler(Command message)
        {
            var inputName = message[CommandProperties.InputSource].ToStringValue();

            var source = "";
            if (inputName == "HDMI")
            {
                source = "KEY_HDMI";
            }
            else if (inputName == "AV")
            {
                source = "KEY_AV1";
            }
            else if (inputName == "COMPONENT")
            {
                source = "KEY_COMPONENT1";
            }
            else if (inputName == "TV")
            {
                source = "KEY_TV";
            }

            if (source?.Length == 0) throw new Exception($"Input {inputName} was not found on Samsung available device input sources");

            await _eventAggregator.QueryAsync<SamsungControlMessage, string>(new SamsungControlMessage
            {
                Address = _hostname,
                Code = source
            });

            _input = await UpdateState(InputSourceState.StateName, _input, inputName);
        }
    }
}