using Quartz;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HA4IoT.Extensions.Quartz
{
    public static class ISchedulerExtensions
    {
        public static async Task<DateTimeOffset> ScheduleInterval<T>(this IScheduler scheduler, TimeSpan interval) where T: IJob
        {
            IJobDetail job = JobBuilder.Create<T>()
              .WithIdentity($"{typeof(T).Name}_{Guid.NewGuid()}")
              .Build();
            
            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity($"{nameof(ScheduleInterval)}_{Guid.NewGuid()}")
                .WithSimpleSchedule(x => x.WithInterval(interval).RepeatForever())
                .Build();
            

            return await scheduler.ScheduleJob(job, trigger);
        }

        public static async Task<JobKey> ScheduleIntervalWithContext<T, D>(this IScheduler scheduler, D data, TimeSpan interval) where T : IJob
        {
            var jobData = new JobDataMap();
            jobData.Add("context", data);
            
            IJobDetail job = JobBuilder.Create<T>()
              .WithIdentity($"{typeof(T).Name}_{Guid.NewGuid()}")
              .SetJobData(jobData)
              .Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity($"{nameof(ScheduleInterval)}_{Guid.NewGuid()}")
                .WithSimpleSchedule(x => x.WithInterval(interval).RepeatForever())
                .Build();

            await scheduler.ScheduleJob(job, trigger);

            return job.Key;
        }
    }
}
