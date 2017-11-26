using System;
using System.Threading.Tasks;
using Wirehome.Components;
using Wirehome.Contracts.Components;
using Wirehome.Contracts.Components.Commands;
using Wirehome.Contracts.Components.Features;
using Wirehome.Contracts.Components.States;
using Wirehome.Extensions.Devices.Commands;
using Wirehome.Extensions.Devices.Features;
using Wirehome.Extensions.Exceptions;
using Wirehome.Extensions.Messaging.Core;
using Wirehome.Extensions.Messaging.SamsungMessages;

namespace Wirehome.Extensions.Devices.Samsung
{
    public class SamsungTV : DeviceComponent
    {     
        private string _hostname;
        
        public string Hostname
        {
            get => _hostname;
            set
            {
                if (_isInitialized) throw new PropertySetAfterInitializationExcption();
                _hostname = value;
            }
        }
        
        public SamsungTV(string id, IEventAggregator eventAggregator) : base(id, eventAggregator)  {}

        public Task Initialize()
        {
            InitPowerStateFeature();
            InitVolumeFeature();
            InitMuteFeature();
            InitInputSourceFeature();
            
            _isInitialized = true;

            return Task.CompletedTask;
        }

        public override IComponentFeatureStateCollection GetState()
        {
            return new ComponentFeatureStateCollection();
            
        }

        #region Power Feature
        private void InitPowerStateFeature()
        {
            _featuresSupported.With(new PowerStateFeature());
            
            //TODO ADD infrared message
            //_commandExecutor.Register<TurnOnCommand>(async c =>
            //{
            //    await _eventAggregator.PublishWithExpectedResultAsync(new SamsungControlMessage
            //    {
            //        Command = "PowerOn",
            //        Api = "formiPhoneAppPower",
            //        ReturnNode = "Power",
            //        Address = Hostname,
            //        Zone = Zone.ToString()
            //    }, "ON").ConfigureAwait(false);
            //    SetPowerState(PowerStateValue.On);
            //});
            _commandExecutor.Register<TurnOffCommand>(async c =>
            {
                await _eventAggregator.QueryAsync<SamsungControlMessage, string>(new SamsungControlMessage
                {
                    Address = Hostname,
                    Code = "KEY_POWEROFF"
                }).ConfigureAwait(false);

            }
            );
        }

        #endregion

        #region Volume Feature
        private void InitVolumeFeature()
        {
            _featuresSupported.With(new VolumeFeature());
            _commandExecutor.Register<VolumeUpCommand>(async c =>
            {
                await _eventAggregator.QueryAsync<SamsungControlMessage, string>(new SamsungControlMessage
                {
                    Address = Hostname,
                    Code = "KEY_VOLUP"
                }).ConfigureAwait(false);
            });
            _commandExecutor.Register<VolumeDownCommand>(async c =>
            {
                await _eventAggregator.QueryAsync<SamsungControlMessage, string>(new SamsungControlMessage
                {
                    Address = Hostname,
                    Code = "KEY_VOLDOWN"
                }).ConfigureAwait(false);
            });

            //TODO Cannon setexact value - maybe it have to be other feature
        }
        #endregion

        #region Mute Feature
        private void InitMuteFeature()
        {
            _featuresSupported.With(new MuteFeature());

            _commandExecutor.Register<MuteOnCommand>(async c =>
            {
                await _eventAggregator.QueryAsync<SamsungControlMessage, string>(new SamsungControlMessage
                {
                    Address = Hostname,
                    Code = "KEY_MUTE"
                }).ConfigureAwait(false);
            });
        }

        #endregion

        #region Input Source Feature
        private void InitInputSourceFeature()
        {
            _featuresSupported.With(new InputSourceFeature());

            _commandExecutor.Register<ChangeInputSourceCommand>(async c =>
            {
                if (c == null) throw new ArgumentNullException();

                var source = "";
                if (c.InputName == "HDMI")
                {
                    source = "KEY_HDMI";
                }
                else if (c.InputName == "AV")
                {
                    source = "KEY_AV1";
                }
                else if (c.InputName == "COMPONENT")
                {
                    source = "KEY_COMPONENT1";
                }
                else if (c.InputName == "TV")
                {
                    source = "KEY_TV";
                }

                if (source?.Length == 0) throw new Exception($"Input {c.InputName} was not found on Samsung available device input sources");

                await _eventAggregator.QueryAsync<SamsungControlMessage, string>(new SamsungControlMessage
                {
                    Address = Hostname,
                    Code = source
                }).ConfigureAwait(false);
            });
        }

     
        #endregion
    }
}
