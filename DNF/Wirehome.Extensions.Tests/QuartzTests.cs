using HA4IoT.Core;
using HA4IoT.Extensions.Quartz;
using Quartz;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HA4IoT.Contracts.Core;
using System.Threading.Tasks;
using System;
using System.Threading;
using Quartz.Impl.Matchers;
using HA4IoT.Extensions.Devices;
using HA4IoT.Extensions.Messaging.Core;

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
            
            var key = await scheduler.ScheduleIntervalWithContext<TestJob, TestContext>(TimeSpan.FromSeconds(1), new TestContext { Data = 5 });

            await scheduler.Start();
    
            await Task.Delay(10000);
        }

        [TestMethod]
        public async Task RegisterQuartzShouldConfigureIoc3()
        {
            var container = new Container(new ControllerOptions());
            container.RegisterSingleton<IContainer>(() => container);
            container.RegisterSingleton<IEventAggregator, EventAggregator>();
            container.RegisterQuartz();

            //container.RegisterType<DenonStateLightJob>();

            CancellationTokenSource _cancelationTokenSource = new CancellationTokenSource();

            var scheduler = container.GetInstance<IScheduler>();

            await scheduler.ScheduleIntervalWithContext<DenonStateLightJob, string>(TimeSpan.FromSeconds(1), "192.168.0.101", _cancelationTokenSource.Token);

            await scheduler.Start();

            await Task.Delay(10000);
        }

        //
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
