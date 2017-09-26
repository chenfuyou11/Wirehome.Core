using System;
using System.Collections.Generic;

namespace Wirehome.Contracts.Logging
{
    public class GetLogEntriesResponse
    {
        public Guid SessionId { get; set; }

        public List<LogEntry> LogEntries { get; set; }
    }
}
