using Wirehome.Contracts.Services;

namespace Wirehome.Contracts.Logging
{
    public interface ILogService : IService
    {
        int WarningsCount { get; }
        int ErrorsCount { get; }
        
        ILogger CreatePublisher(string source);
    }
}
