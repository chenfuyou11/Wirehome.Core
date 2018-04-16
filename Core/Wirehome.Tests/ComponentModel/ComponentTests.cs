using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Quartz;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wirehome.ComponentModel.Commands;
using Wirehome.ComponentModel.Components;
using Wirehome.Core.EventAggregator;
using Wirehome.Core.Services.Logging;
using Wirehome.Core.Tests.ComponentModel;

namespace Wirehome.Extensions.Tests
{
    [TestClass]
    public class ComponentTests : ReactiveTest
    {
        [TestMethod]
        public async Task ComponentCommandExecuteShouldGetResult()
        {
            var container = CommonIntegrationcs.PrepareContainer();
            var eventAggregator = container.GetInstance<IEventAggregator>();
            var logger = container.GetInstance<ILogService>();
            var quartz = container.GetInstance<ISchedulerFactory>();
            var component = new Component(eventAggregator, logger, quartz);
            await component.Initialize();

            var result = await component.ExecuteCommand<IEnumerable<string>>(new Command(CommandType.SupportedCapabilitiesCommand));
        }
    }
}