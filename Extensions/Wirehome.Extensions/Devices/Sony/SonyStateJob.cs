using Quartz;
using System;
using System.Threading.Tasks;
using Wirehome.Contracts.Logging;
using Wirehome.Extensions.Messaging.Core;
using Wirehome.Extensions.Messaging.SonyMessages;

namespace Wirehome.Extensions.Devices.Sony
{
    public class SonyStateJob : IJob
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly ILogger _logger;

        public SonyStateJob(IEventAggregator eventAggregator, ILogService logService)
        {
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            _logger = (logService ?? throw new ArgumentNullException(nameof(logService))).CreatePublisher(nameof(SonyStateJob));
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                if (context.CancellationToken.IsCancellationRequested) return;

                if (context.JobDetail.JobDataMap.TryGetValue("context", out object contextData))
                {
                    var stateJobContext = contextData as SonyStateJobContext;

                    var power = await _eventAggregator.PublishWithResultAsync<SonyJsonMessage, string>(new SonyJsonMessage
                    {
                        Address = stateJobContext.Hostname,
                        AuthorisationKey = stateJobContext.AuthKey,
                        Path = "system",
                        Method = "getPowerStatus"
                    }).ConfigureAwait(false);

                    var aydio = await _eventAggregator.PublishWithResultAsync<SonyJsonMessage, string>(new SonyJsonMessage
                    {
                        Address = stateJobContext.Hostname,
                        AuthorisationKey = stateJobContext.AuthKey,
                        Path = "audio",
                        Method = "getVolumeInformation"
                    }).ConfigureAwait(false);

                    // TODO Send result
                }
            }
            catch (Exception ee)
            {
                _logger.Error(ee, $"Unhandled exception in {nameof(SonyStateJob)}");
            }
        }
    }
}
