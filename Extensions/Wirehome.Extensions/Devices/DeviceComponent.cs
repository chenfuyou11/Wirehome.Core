using Wirehome.Components;
using System;
using Wirehome.Contracts.Components;
using Wirehome.Contracts.Components.Commands;
using Wirehome.Extensions.Messaging.Core;
using System.Threading.Tasks;
using Wirehome.Contracts.Components.States;

namespace Wirehome.Extensions.Devices
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
