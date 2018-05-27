﻿using Quartz;
using Quartz.Spi;
using System;
using Wirehome.Core.Services.DependencyInjection;

namespace Wirehome.Core.Services.Quartz
{
    public class SimpleInjectorJobFactory : IJobFactory
    {
        private readonly IContainer container;

        public SimpleInjectorJobFactory(IContainer container)
        {
            this.container = container ?? throw new ArgumentNullException(nameof(container));
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            var jobDetail = bundle.JobDetail;
            var jobType = jobDetail.JobType;

            try
            {
                return new JobWrapper(bundle, container);
            }
            catch (Exception ex)
            {
                throw new SchedulerException($"Problem instantiating class '{jobDetail.JobType.FullName}'", ex);
            }
        }

        public void ReturnJob(IJob job)
        {
            (job as IDisposable)?.Dispose();
        }
    }
}
