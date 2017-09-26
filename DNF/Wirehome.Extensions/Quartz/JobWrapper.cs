using Quartz.Spi;
using System;
using Quartz;
using HA4IoT.Contracts.Core;
using System.Threading.Tasks;

namespace HA4IoT.Extensions.Quartz
{
    internal class JobWrapper : IJob
    {
        private readonly TriggerFiredBundle bundle;
        private readonly IContainer container;

        public JobWrapper(TriggerFiredBundle bundle, IContainer container)
        {
            this.bundle = bundle;
            this.container = container;
        }

        protected IJob RunningJob { get; private set; }

        public Task Execute(IJobExecutionContext context)
        {
            try
            {
                RunningJob = container.GetInstance(bundle.JobDetail.JobType) as IJob;
                return RunningJob.Execute(context);
            }
            catch (JobExecutionException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new JobExecutionException($"Failed to execute Job '{bundle.JobDetail.Key}' of type '{bundle.JobDetail.JobType}'", ex);
            }
            finally
            {
                RunningJob = null;
            }
        }

    }

   

}
