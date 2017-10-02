using System.Threading.Tasks;
using Wirehome.Components;
using Wirehome.Contracts.Components;
using Wirehome.Contracts.Components.Commands;
using Wirehome.Contracts.Components.Features;
using Wirehome.Extensions.Devices.Commands;
using Wirehome.Extensions.Devices.Features;
using Wirehome.Extensions.Exceptions;
using Wirehome.Extensions.Messaging.Core;
using Wirehome.Extensions.Messaging.SonyMessages;

namespace Wirehome.Extensions.Devices.Sony
{
    public class SonyBraviaTV : DeviceComponent
    {
        
        private string _hostname;
        private string _authorisationKey;

        public string Hostname
        {
            get => _hostname;
            set
            {
                if (_isInitialized) throw new PropertySetAfterInitializationExcption();
                _hostname = value;
            }
        }

        public string AuthorisationKey
        {
            get => _authorisationKey;
            set
            {
                if (_isInitialized) throw new PropertySetAfterInitializationExcption();
                _authorisationKey = value;
            }
        }

        public SonyBraviaTV(string id, IEventAggregator eventAggregator) : base(id, eventAggregator) { }

        public Task Initialize()
        {
            InitPowerStateFeature();
            InitVolumeFeature();
            //InitMuteFeature();
            //InitInputSourceFeature();

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

            _commandExecutor.Register<TurnOnCommand>(async c =>
            {
                var result = await _eventAggregator.PublishWithResultAsync<SonyControlMessage, string>(new SonyControlMessage
                {
                    Address = Hostname,
                    AuthorisationKey = AuthorisationKey,
                    Code = "AAAAAQAAAAEAAAAuAw=="
                }).ConfigureAwait(false);
            }
           );
            _commandExecutor.Register<TurnOffCommand>(async c =>
            {
                var result = await _eventAggregator.PublishWithResultAsync<SonyControlMessage, string>(new SonyControlMessage
                {
                    Address = Hostname,
                    AuthorisationKey = AuthorisationKey,
                    Code = "AAAAAQAAAAEAAAAvAw=="
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
                var result = await _eventAggregator.PublishWithResultAsync<SonyControlMessage, string>(new SonyControlMessage
                {
                    Address = Hostname,
                    AuthorisationKey = AuthorisationKey,
                    Code = "AAAAAQAAAAEAAAASAw=="
                }).ConfigureAwait(false);
            }
          );
            _commandExecutor.Register<VolumeDownCommand>(async c =>
            {
                var result = await _eventAggregator.PublishWithResultAsync<SonyControlMessage, string>(new SonyControlMessage
                {
                    Address = Hostname,
                    AuthorisationKey = AuthorisationKey,
                    Code = "AAAAAQAAAAEAAAATAw=="
                }).ConfigureAwait(false);
            }
            );
        }
        #endregion
    }
}
