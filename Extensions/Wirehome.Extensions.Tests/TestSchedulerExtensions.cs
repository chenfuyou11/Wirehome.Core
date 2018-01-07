using System.Reactive.Linq;
using Microsoft.Reactive.Testing;
using System.Linq;
using System;

namespace Wirehome.Extensions.Tests
{
    public static class TestExtensions
    {
        public static void AdvanceToEnd<T>(this TestScheduler scheduler, ITestableObservable<T> events)
        {
            scheduler.AdvanceTo(events.Messages.Max(x => x.Time));
        }
        
        public static void AdvanceToBeyondEnd<T>(this TestScheduler scheduler, ITestableObservable<T> events, int beyondEnd = 500)
        {
            scheduler.AdvanceTo(events.Messages.Max(x => x.Time) + Time.Tics(beyondEnd));
        }

        public static TimeSpan JustAfter(this TimeSpan span, int timeAfter = 100) => span.Add(TimeSpan.FromMilliseconds(timeAfter));
        
    }


}
