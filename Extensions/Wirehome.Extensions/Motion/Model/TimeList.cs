using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Text;

namespace Wirehome.Motion.Model
{
    //TODO Add thread safe
    public class TimeList : IEnumerable<DateTimeOffset>
    {
        private List<DateTimeOffset> _innerList { get; } = new List<DateTimeOffset>();
        private readonly IScheduler _scheduler;

        public TimeList(IScheduler scheduler)
        {
            _scheduler = scheduler;
        }
        public void Add(DateTimeOffset time)
        {
            _innerList.Add(time);
        }

        public IEnumerable<DateTimeOffset> GetLastElements(DateTimeOffset endTime, TimeSpan period)
        {
            return _innerList.Where(el => el < endTime && endTime - el < period);
        }

        public bool HasElement(TimeSpan period)
        {
            return _innerList.Any(el => _scheduler.Now - el < period);
        }

        public void ClearOldData(TimeSpan period)
        {
            _innerList.RemoveAll(el => _scheduler.Now - el > period);
        }

        public IEnumerator<DateTimeOffset> GetEnumerator()
        {
            return _innerList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
