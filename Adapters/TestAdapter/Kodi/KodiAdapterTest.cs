using System;
using System.Threading.Tasks;
using Wirehome.ComponentModel.Capabilities;
using Wirehome.ComponentModel.Commands;
using Wirehome.ComponentModel.Commands.Responses;
using Wirehome.ComponentModel.ValueTypes;
using Wirehome.Core.Extensions;

namespace Wirehome.ComponentModel.Adapters.Kodi
{
    public class KodiAdapterTest : Adapter
    {
        public const int DEFAULT_POOL_INTERVAL = 1000;

        private string _hostname;
        private TimeSpan _poolInterval;
        private int _port;
        private string _userName;
        private string _Password;

        private BooleanValue _powerState;
        private DoubleValue _volume;
        private BooleanValue _mute;
        private DoubleValue _speed;

        //TODO read this value in refresh?
        private int? PlayerId { get; }

        public KodiAdapterTest(IAdapterServiceFactory adapterServiceFactory) : base(adapterServiceFactory)
        {
        }

        public override Task Initialize()
        {
            return base.Initialize();
        }

        protected async Task RefreshCommandHandler(Command message)
        {
            //TODO pool state
        }

        protected DiscoveryResponse DiscoverCapabilitiesHandler(Command message)
        {
            return new DiscoveryResponse(RequierdProperties(), new PowerState(),
                                                               new VolumeState(),
                                                               new MuteState(),
                                                               new PlaybackState()
                                          );
        }

        protected string TestCommandHandler(Command message)
        {
            return "Dominik";
        }

        protected async Task TurnOffCommandHandler(Command message)
        {
            var result = await _eventAggregator.QueryAsync<KodiMessage, string>(new KodiMessage
            {
                Address = _hostname,
                UserName = _userName,
                Password = _Password,
                Port = _port,
                Method = "Application.Quit"
            });
            _powerState = await UpdateState(PowerState.StateName, _powerState, new BooleanValue(false));
        }

        protected async Task VolumeUpCommandHandler(Command command)
        {
            var volume = _volume + command[CommandProperties.ChangeFactor].ToDoubleValue();

            var result = await _eventAggregator.QueryAsync<KodiMessage, string>(new KodiMessage
            {
                Address = _hostname,
                UserName = _userName,
                Password = _Password,
                Port = _port,
                Method = "Application.SetVolume",
                Parameters = new { volume = (int)volume }
            });

            _volume = await UpdateState(VolumeState.StateName, _volume, new DoubleValue(volume));
        }

        protected async Task VolumeDownCommandHandler(Command command)
        {
            var volume = _volume + command[CommandProperties.ChangeFactor].ToDoubleValue();

            var result = await _eventAggregator.QueryAsync<KodiMessage, string>(new KodiMessage
            {
                Address = _hostname,
                UserName = _userName,
                Password = _Password,
                Port = _port,
                Method = "Application.SetVolume",
                Parameters = new { volume = (int)volume }
            });

            _volume = await UpdateState(VolumeState.StateName, _volume, new DoubleValue(volume));
        }

        protected async Task VolumeSetCommandHandler(Command command)
        {
            var volume = command[CommandProperties.Value].ToDoubleValue();
            var result = await _eventAggregator.QueryAsync<KodiMessage, string>(new KodiMessage
            {
                Address = _hostname,
                UserName = _userName,
                Password = _Password,
                Port = _port,
                Method = "Application.SetVolume",
                Parameters = new { volume = volume }
            });

            _volume = await UpdateState(VolumeState.StateName, _volume, new DoubleValue(volume));
        }

        protected async Task MuteCommandHandler(Command message)
        {
            var result = await _eventAggregator.QueryAsync<KodiMessage, string>(new KodiMessage
            {
                Address = _hostname,
                UserName = _userName,
                Password = _Password,
                Port = _port,
                Method = "Application.SetMute",
                Parameters = new { mute = true }
            });

            _mute = await UpdateState(MuteState.StateName, _mute, new BooleanValue(true));
        }

        protected async Task UnmuteCommandHandler(Command message)
        {
            var result = await _eventAggregator.QueryAsync<KodiMessage, string>(new KodiMessage
            {
                Address = _hostname,
                UserName = _userName,
                Password = _Password,
                Port = _port,
                Method = "Application.SetMute",
                Parameters = new { mute = false }
            });

            _mute = await UpdateState(MuteState.StateName, _mute, new BooleanValue(false));
        }

        protected async Task PlayCommandHandler(Command message)
        {
            if (_speed != 0) return;

            //{"jsonrpc": "2.0", "method": "Player.PlayPause", "params": { "playerid": 1 }, "id": 1}
            var result = await _eventAggregator.QueryAsync<KodiMessage, string>(new KodiMessage
            {
                Address = _hostname,
                UserName = _userName,
                Password = _Password,
                Port = _port,
                Method = "Player.PlayPause",
                Parameters = new { playerid = PlayerId.GetValueOrDefault() }
            });

            _speed = await UpdateState(PlaybackState.StateName, _speed, new DoubleValue(1.0));
        }

        protected async Task StopCommandHandler(Command message)
        {
            if (_speed != 0) return;

            //{ "jsonrpc": "2.0", "method": "Player.Stop", "id": "libMovies", "params": { "playerid": 1 } }
            var result = await _eventAggregator.QueryAsync<KodiMessage, string>(new KodiMessage
            {
                Address = _hostname,
                UserName = _userName,
                Password = _Password,
                Port = _port,
                Method = "Player.Stop",
                Parameters = new { playerid = PlayerId.GetValueOrDefault() }
            });

            _speed = await UpdateState(PlaybackState.StateName, _speed, new DoubleValue(1.0));
        }
    }
}