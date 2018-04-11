using System;
using Wirehome.Contracts.Services;

namespace Wirehome.Contracts.Core
{
    public interface IDateTimeService : IService
    {
        DateTime Date { get; }

        TimeSpan Time { get; }

        DateTime Now { get; }
    }
}
