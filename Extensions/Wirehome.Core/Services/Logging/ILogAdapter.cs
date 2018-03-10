namespace Wirehome.Core.Services.Logging
{
    public interface ILogAdapter
    {
        void ProcessLogEntry(LogEntry logEntry);
    }
}
