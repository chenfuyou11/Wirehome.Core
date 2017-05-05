using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;
using System;
using Windows.Foundation.Diagnostics;

namespace HA4IoT.Extensions
{
    public class EtwLoggingService : ServiceBase, IEtwLoggingService
    {
        LoggingChannel _loggingChannel;

        public EtwLoggingService(ILogService logService)
        {
            _loggingChannel = new LoggingChannel("HA4IoT", null, new Guid("4bd2826e-54a1-4ba9-bf63-92b73ea1ac4a"));

            logService.LogEntryPublished += LogService_LogEntryPublished;
        }

        private void LogService_LogEntryPublished(object sender, LogEntryPublishedEventArgs e)
        {
            var fields = new LoggingFields();


            fields.AddStringArray("Source", new string[] { e.LogEntry.Source ?? "" });
            
            //fields.AddString("Exception", e.LogEntry.Exception ?? "");
            //fields.AddString("Source", e.LogEntry.Source ?? "");

            _loggingChannel.LogEvent(e.LogEntry.Message, fields, MapSeverity(e.LogEntry.Severity));

          
        }

        private static LoggingLevel MapSeverity(LogEntrySeverity severity)
        {
            var level = LoggingLevel.Information;
            if (severity == LogEntrySeverity.Error)
            {
                level = LoggingLevel.Error;
            }
            else if (severity == LogEntrySeverity.Verbose)
            {
                level = LoggingLevel.Verbose;
            }
            else if (severity == LogEntrySeverity.Warning)
            {
                level = LoggingLevel.Warning;
            }

            return level;
        }
    }
}
