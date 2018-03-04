using Quartz;
using System;
using Wirehome.Core;
using Wirehome.Core.Communication.I2C;
using Wirehome.Core.EventAggregator;

namespace Wirehome.ComponentModel.Adapters
{
    public class AdapterServiceFactory : IAdapterServiceFactory
    {
        protected readonly ILogger _log;
        protected readonly II2CBusService _i2CBusService;
        protected readonly IEventAggregator _eventAggregator;
        protected readonly IScheduler _scheduler;

        protected AdapterServiceFactory(IEventAggregator eventAggregator, IScheduler scheduler, II2CBusService i2CBusService, ILogger log) 
        {
            _i2CBusService = i2CBusService ?? throw new ArgumentNullException(nameof(i2CBusService));
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
        }

        public ILogger GetLogger() => _log;
        public II2CBusService GetI2CService() => _i2CBusService;
        public IEventAggregator GetEventAggregator() => _eventAggregator;
        public IScheduler GetScheduler() => _scheduler;
    }
}
