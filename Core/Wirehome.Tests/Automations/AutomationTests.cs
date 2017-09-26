using System;
using Wirehome.Actuators.Lamps;
using Wirehome.Automations;
using Wirehome.Components;
using Wirehome.Conditions;
using Wirehome.Conditions.Specialized;
using Wirehome.Contracts.Components.States;
using Wirehome.Contracts.Core;
using Wirehome.Contracts.Logging;
using Wirehome.Contracts.Messaging;
using Wirehome.Contracts.Settings;
using Wirehome.Sensors.Buttons;
using Wirehome.Tests.Mockups;
using Wirehome.Tests.Mockups.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wirehome.Tests.Automations
{
    [TestClass]
    public class AutomationTests
    {
        [TestMethod]
        public void Automation_Toggle()
        {
            var c = new TestController();

            var buttonAdapter = new TestButtonAdapter();
            var button = new Button("Test", buttonAdapter, c.GetInstance<ITimerService>(), c.GetInstance<ISettingsService>(), c.GetInstance<IMessageBrokerService>(), c.GetInstance<ILogService>());
            var testOutput = new Lamp("Test", new TestLampAdapter());
            
            new Automation("Test")
                .WithTrigger(button.CreatePressedShortTrigger(c.GetInstance<IMessageBrokerService>()))
                .WithActionIfConditionsFulfilled(() => testOutput.TryTogglePowerState());

            Assert.IsTrue(testOutput.GetState().Has(PowerState.Off));
            buttonAdapter.Touch();
            Assert.IsTrue(testOutput.GetState().Has(PowerState.On));
            buttonAdapter.Touch();
            Assert.IsTrue(testOutput.GetState().Has(PowerState.Off));
            buttonAdapter.Touch();
            Assert.IsTrue(testOutput.GetState().Has(PowerState.On));
        }

        [TestMethod]
        public void Automation_WithCondition()
        {
            var c = new TestController();

            var buttonAdapter = new TestButtonAdapter();
            var button = new Button("Test", buttonAdapter, c.GetInstance<ITimerService>(), c.GetInstance<ISettingsService>(), c.GetInstance<IMessageBrokerService>(), c.GetInstance<ILogService>());
            var testOutput = new Lamp("Test", new TestLampAdapter());

            new Automation("Test")
                .WithTrigger(button.CreatePressedShortTrigger(c.GetInstance<IMessageBrokerService>()))
                .WithCondition(ConditionRelation.And, new TimeRangeCondition(c.GetInstance<IDateTimeService>()).WithStart(TimeSpan.FromHours(1)).WithEnd(TimeSpan.FromHours(2)))
                .WithActionIfConditionsFulfilled(() => testOutput.TryTogglePowerState());

            Assert.IsTrue(testOutput.GetState().Has(PowerState.Off));
            c.SetTime(TimeSpan.FromHours(0));
            buttonAdapter.Touch();
            Assert.IsTrue(testOutput.GetState().Has(PowerState.Off));

            c.SetTime(TimeSpan.FromHours(1.5));
            buttonAdapter.Touch();
            Assert.IsTrue(testOutput.GetState().Has(PowerState.On));
        }
    }
}
