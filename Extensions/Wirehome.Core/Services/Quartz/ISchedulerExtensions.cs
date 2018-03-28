using Quartz;
using Quartz.Impl.Matchers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Wirehome.Core.Services.Quartz
{
    public static class QuartzExtensions
    {
        public static async Task<JobKey> ScheduleInterval<T>(this IScheduler scheduler, TimeSpan interval, CancellationToken token = default) where T : IJob
        {
            IJobDetail job = JobBuilder.Create<T>()
              .WithIdentity($"{typeof(T).Name}_{Guid.NewGuid()}")
              .Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity($"{nameof(ScheduleInterval)}_{Guid.NewGuid()}")
                .WithSimpleSchedule(x => x.WithInterval(interval).RepeatForever())
                .Build();

            await scheduler.ScheduleJob(job, trigger, token);

            return job.Key;
        }

        public static void AddListner(this IScheduler scheduler, IJobListener listner, JobKey key)
        {
            scheduler.ListenerManager.AddJobListener(listner, KeyMatcher<JobKey>.KeyEquals(key));
        }

        public static async Task<JobKey> ScheduleIntervalWithContext<T, D>(this IScheduler scheduler, TimeSpan interval, D data, CancellationToken token = default) where T : IJob
        {
            var jobData = new JobDataMap
            {
                { "context", data }
            };

            IJobDetail job = JobBuilder.Create<T>()
                                       .WithIdentity($"{typeof(T).Name}_{Guid.NewGuid()}")
                                       .SetJobData(jobData)
                                       .Build();

            ITrigger trigger = TriggerBuilder.Create()
                                             .WithIdentity($"{nameof(ScheduleInterval)}_{Guid.NewGuid()}")
                                             .WithSimpleSchedule(x => x.WithInterval(interval).RepeatForever())
                                             .Build();

            await scheduler.ScheduleJob(job, trigger, token);

            return job.Key;
        }

        public static T GetDataContext<T>(this IJobExecutionContext context) where T : class
        {
            if (context.JobDetail.JobDataMap.TryGetValue("context", out object value))
            {
                return value as T;
            }
            return default;
        }
    }
}