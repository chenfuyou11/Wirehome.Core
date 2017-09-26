using System;
using Wirehome.Extensions.Messaging.DenonMessages;
using Wirehome.Extensions.Messaging.Core;
using Quartz;
using System.Threading.Tasks;
using Wirehome.Contracts.Logging;

namespace Wirehome.Extensions.Devices
{
    public class DenonStateLightJob : IJob
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly ILogger _logger;

        public DenonStateLightJob(IEventAggregator eventAggregator, ILogService logService)
        {
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            _logger = (logService ?? throw new ArgumentNullException(nameof(logService))).CreatePublisher(nameof(DenonStateLightJob));
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                if (context.CancellationToken.IsCancellationRequested) return;
                
                await _eventAggregator.PublishWithRepublishResult<DenonStatusLightMessage, DenonStatus>(new DenonStatusLightMessage
                {
                    Address = context.JobDetail.JobDataMap.GetString("context")
                }).ConfigureAwait(false);
            }
            catch (Exception ee)
            {
                _logger.Error(ee, $"Unhandled exception in {nameof(DenonStateLightJob)}");
            }
            
        }
    }
}
