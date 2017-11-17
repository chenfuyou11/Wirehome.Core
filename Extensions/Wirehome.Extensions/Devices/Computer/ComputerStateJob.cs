using System;
using System.Threading.Tasks;
using Quartz;
using Wirehome.Extensions.Messaging.DenonMessages;
using Wirehome.Extensions.Messaging.Core;
using Wirehome.Contracts.Logging;
using Wirehome.Extensions.Devices.Denon;
using Wirehome.Extensions.Messaging.ComputerMessages;

namespace Wirehome.Extensions.Devices
{
    public class ComputerStateJob : IJob
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly ILogger _logger;

        public ComputerStateJob(IEventAggregator eventAggregator, ILogService logService)
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
                    var computerStateJobContext = contextData as ComputerStateJobContext;

                    await _eventAggregator.SendWithRepublishResult<ComputerControlMessage, ComputerStatus>(new ComputerControlMessage
                    {
                        Address = computerStateJobContext.Hostname,
                        Port = computerStateJobContext.Port,
                        Service = "Status",
                        RequestType = "GET"
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
