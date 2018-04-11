namespace Wirehome.Contracts.Logging
{
    public interface ILogAdapter
    {
        void ProcessLogEntry(LogEntry logEntry);
    }
}
