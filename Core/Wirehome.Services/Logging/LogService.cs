using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Wirehome.Core.Services.Logging
{
    public sealed class LogService : ILogService
    {
        public ILogger CreatePublisher(string source)
        {
            return new LogServicePublisher(source, this);
        }

        public void Dispose()
        {
            
        }

        public Task Initialize()
        {
            return Task.CompletedTask;
        }

        public void Publish(LogEntrySeverity severity, string source, string message, Exception exception)
        {

        }
    }
}
