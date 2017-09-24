using Quartz.Spi;
using System;
using System.Collections.Generic;
using System.Text;
using Quartz;
using SimpleInjector;
using HA4IoT.Contracts.Core;
using System.Globalization;

namespace HA4IoT.Extensions.Quartz
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
