using System;
using System.Threading.Tasks;
using Quartz;
using Wirehome.Extensions.Messaging.DenonMessages;
using Wirehome.Extensions.Messaging.Core;
using Wirehome.Contracts.Logging;
using Wirehome.Extensions.Devices.Denon;

namespace Wirehome.Extensions.Devices
{
    public class DenonStateJob : IJob
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly ILogger _logger;

        public DenonStateJob(IEventAggregator eventAggregator, ILogService logService)
        {
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            _logger = (logService ?? throw new ArgumentNullException(nameof(logService))).CreatePublisher(nameof(DenonStateJob));
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                if (context.CancellationToken.IsCancellationRequested) return;

                if(context.JobDetail.JobDataMap.TryGetValue("context", out object contextData))
                {
                    var denonStateJobContext = contextData as DenonStateJobContext;
                    await _eventAggregator.PublishWithRepublishResult<DenonStatusLightMessage, DenonStatus>(new DenonStatusLightMessage
                    {
                        Address = denonStateJobContext.Hostname,
                        Zone = denonStateJobContext.Zone
                    }).ConfigureAwait(false);
                }
            }
            catch (Exception ee)
            {
                _logger.Error(ee, $"Unhandled exception in {nameof(DenonStateJob)}");
            }
        }
    }
}
