using System;
using System.Collections.Generic;
using Wirehome.ComponentModel.Commands;
using Wirehome.ComponentModel.Events;
using Wirehome.Core.Services.DependencyInjection;

namespace Wirehome.ComponentModel.Components
{
    public class Trigger
    {
        public Event Event { get; set; }
        public Command Command { get; set; }
        public Command FinishCommand { get; set; }
        public Schedule Schedule { get; set; }
    }

    public class Schedule
    {
        public string CronExpression { get; set; }
        public string Calendar { get; set; }
        public IList<ManualSchedule> ManualSchedules { get; set; }
    }

    public class ManualSchedule
    {
        private DateTime? _finish;

        [Map] public DateTime Start { get; private set; }
        [Map]
        public DateTime Finish
        {
            get
            {
                if(!_finish.HasValue && WorkingTime.HasValue)
                {
                    _finish = Start.Add(WorkingTime.Value);
                }
                return _finish.Value; }
            private set { _finish = value; }
        }
        [Map] public TimeSpan? WorkingTime { get; private set; }
    }
}