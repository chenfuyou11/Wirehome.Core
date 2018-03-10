using Quartz;
using System;
using Wirehome.Core.Communication.I2C;
using Wirehome.Core.EventAggregator;
using Wirehome.Core.Services.Logging;

namespace Wirehome.ComponentModel.Adapters
{
    public class AdapterServiceFactory : IAdapterServiceFactory
    {
        protected readonly II2CBusService _i2CBusService;
        protected readonly IEventAggregator _eventAggregator;
        protected readonly IScheduler _scheduler;
        private readonly ILogService _logService;

        public AdapterServiceFactory(IEventAggregator eventAggregator, IScheduler scheduler, II2CBusService i2CBusService, ILogService logService)
        {
            _i2CBusService = i2CBusService ?? throw new ArgumentNullException(nameof(i2CBusService));
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
        }

        public ILogService GetLogger() => _logService;
        public II2CBusService GetI2CService() => _i2CBusService;
        public IEventAggregator GetEventAggregator() => _eventAggregator;
        public IScheduler GetScheduler() => _scheduler;
    }
}
