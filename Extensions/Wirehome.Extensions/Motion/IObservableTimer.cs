using System;

namespace Wirehome.Extensions
{
    public interface IObservableTimer
    {
        IObservable<DateTimeOffset> GenerateTime(TimeSpan period);
    }
}