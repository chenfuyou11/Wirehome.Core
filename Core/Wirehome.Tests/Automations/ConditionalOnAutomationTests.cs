using Wirehome.Actuators.Lamps;
using Wirehome.Automations;
using Wirehome.Contracts.Components.States;
using Wirehome.Contracts.Core;
using Wirehome.Contracts.Environment;
using Wirehome.Contracts.Logging;
using Wirehome.Contracts.Messaging;
using Wirehome.Contracts.Scheduling;
using Wirehome.Contracts.Services;
using Wirehome.Contracts.Settings;
using Wirehome.Sensors.Buttons;
using Wirehome.Tests.Mockups;
using Wirehome.Tests.Mockups.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wirehome.Tests.Automations
{
    [TestClass]
    public class ConditionalOnAutomationTests
    {
        [TestMethod]
        public void Empty_ConditionalOnAutomation()
        {
            var c = new TestController();

            var automation = new ConditionalOnAutomation("Test",
                c.GetInstance<ISchedulerService>(),
                c.GetInstance<IDateTimeService>(),
                c.GetInstance<IDaylightService>());

            var buttonAdapter = new TestButtonAdapter();
            var button = new Button("Test", buttonAdapter, c.GetInstance<ITimerService>(), c.GetInstance<ISettingsService>(), c.GetInstance<IMessageBrokerService>(), c.GetInstance<ILogService>());
            var testOutput = new Lamp("Test", new TestLampAdapter());

            automation.WithTrigger(button.CreatePressedShortTrigger(c.GetInstance<IMessageBrokerService>()));
            automation.WithComponent(testOutput);

            Assert.IsTrue(testOutput.GetState().Has(PowerState.Off));
            buttonAdapter.Touch();
            Assert.IsTrue(testOutput.GetState().Has(PowerState.On));
        }
    }
}
