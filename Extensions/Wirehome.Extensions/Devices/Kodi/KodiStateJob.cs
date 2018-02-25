using Quartz;
using System;
using System.Threading.Tasks;
using Wirehome.Contracts.Logging;
using Wirehome.Core.EventAggregator;
using Wirehome.Extensions.Messaging.Core;

namespace Wirehome.Extensions.Devices.Kodi
{
    public class KodiStateJob : IJob
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly ILogger _logger;

        public KodiStateJob(IEventAggregator eventAggregator, ILogService logService)
        {
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            _logger = (logService ?? throw new ArgumentNullException(nameof(logService))).CreatePublisher(nameof(KodiStateJob));
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                if (context.CancellationToken.IsCancellationRequested) return;

                if (context.JobDetail.JobDataMap.TryGetValue("context", out object contextData))
                {
                    var stateJobContext = contextData as KodiStateJobContext;

                    // TODO Send result
                }
            }
            catch (Exception ee)
            {
                _logger.Error(ee, $"Unhandled exception in {nameof(KodiStateJob)}");
            }
        }
    }
}
