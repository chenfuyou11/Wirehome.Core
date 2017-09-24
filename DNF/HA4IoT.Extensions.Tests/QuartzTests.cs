using HA4IoT.Core;
using HA4IoT.Extensions.Quartz;
using Quartz;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HA4IoT.Contracts.Core;
using System.Threading.Tasks;
using System;
using System.Threading;
using Quartz.Impl.Matchers;

namespace HA4IoT.Extensions.Tests
{
    [TestClass]
    public class QuartzTests
    {
        [TestMethod]
        public void RegisterQuartzShouldConfigureIoc()
        {
            var container = new Container(new ControllerOptions());
            container.RegisterSingleton<IContainer>(() => container);
            container.RegisterQuartz();
            
            var scheduler = container.GetInstance<IScheduler>();
        }

        [TestMethod]
        public async Task RegisterQuartzShouldConfigureIoc2()
        {
            var container = new Container(new ControllerOptions());
            container.RegisterSingleton<IContainer>(() => container);
            container.RegisterQuartz();

            var scheduler = container.GetInstance<IScheduler>();
            
            var key = await scheduler.ScheduleIntervalWithContext<TestJob, TestContext>(new TestContext { Data = 5 }, TimeSpan.FromSeconds(1));

            await scheduler.Start();
            
            scheduler.ListenerManager.AddJobListener(new Listner { Name = "Listnerek" }, KeyMatcher<JobKey>.KeyEquals(key));
            
            await Task.Delay(10000);
        }
    }

    public class Listner : IJobListener
    {
        public string Name { get; set; }

        public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.CompletedTask;
        }

        public Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.CompletedTask;
        }

        public Task JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.CompletedTask;
        }
    }

    public class TestContext
    {
        public int Data { get; set; }
    }

    public class TestJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            if(context.JobDetail.JobDataMap.TryGetValue("context", out object value))
            {

            }

            context.Result = new TestContext { Data = 5 };

            return Task.CompletedTask;
        }
    }
}
