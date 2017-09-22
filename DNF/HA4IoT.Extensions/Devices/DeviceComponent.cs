using HA4IoT.Components;
using System;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Components.Commands;
using HA4IoT.Extensions.Messaging.Core;
using System.Threading.Tasks;
using HA4IoT.Contracts.Components.States;

namespace HA4IoT.Extensions.Devices
{
    public abstract class DeviceComponent : ComponentBase, ICommandExecute
    {
        protected readonly AsyncCommandExecutor _commandExecutor;
        protected readonly IEventAggregator _eventAggregator;
        protected readonly IComponentFeatureCollection _featuresSupported;
        protected readonly IComponentFeatureStateCollection _componentStates;

        public DeviceComponent(string id, IEventAggregator eventAggregator) : base(id)
        {
            _commandExecutor = new AsyncCommandExecutor();
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            _featuresSupported = new ComponentFeatureCollection();
        }

        public async Task ExecuteAsyncCommand<T>() where T : ICommand => await _commandExecutor.Execute<T>().ConfigureAwait(false);

        public override IComponentFeatureCollection GetFeatures() => _featuresSupported;
        
    }

}
