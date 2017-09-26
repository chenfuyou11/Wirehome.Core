using System;
using Wirehome.Contracts.Services;

namespace Wirehome.Contracts.Environment
{
    public interface IDaylightService : IService
    {
        TimeSpan Sunrise { get; }
        TimeSpan Sunset { get; }
        DateTime? Timestamp { get; }
        void Update(TimeSpan sunrise, TimeSpan sunset);
    }
}
