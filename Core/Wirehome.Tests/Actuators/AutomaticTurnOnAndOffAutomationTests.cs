using System;
using Wirehome.Actuators.Lamps;
using Wirehome.Automations;
using Wirehome.Components;
using Wirehome.Contracts.Components;
using Wirehome.Contracts.Components.States;
using Wirehome.Contracts.Core;
using Wirehome.Contracts.Environment;
using Wirehome.Contracts.Logging;
using Wirehome.Contracts.Messaging;
using Wirehome.Contracts.Scheduling;
using Wirehome.Contracts.Services;
using Wirehome.Contracts.Settings;
using Wirehome.Sensors.Buttons;
using Wirehome.Sensors.MotionDetectors;
using Wirehome.Tests.Mockups;
using Wirehome.Tests.Mockups.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wirehome.Tests.Actuators
{
    [TestClass]
    public class AutomaticTurnOnAndOffAutomationTests
    {
        [TestMethod]
        public void Should_TurnOn_IfMotionDetected()
        {
            var testController = new TestController();
            var adapter = new TestMotionDetectorAdapter();
            var motionDetector = new MotionDetector(
                "Test", 
                adapter, 
                testController.GetInstance<ISchedulerService>(),
                testController.GetInstance<ISettingsService>(),
                testController.GetInstance<IMessageBrokerService>());

            var automation = new TurnOnAndOffAutomation(
                "Test",
                testController.GetInstance<IDateTimeService>(), 
                testController.GetInstance<ISchedulerService>(),
                testController.GetInstance<ISettingsService>(),
                testController.GetInstance<IDaylightService>(),
                testController.GetInstance<IMessageBrokerService>());

            var output = new Lamp("Test", new TestLampAdapter());
            Assert.AreEqual(true, output.GetState().Has(PowerState.Off));

            automation.WithTrigger(motionDetector);
            automation.WithTarget(output);

            adapter.Invoke();

            Assert.AreEqual(true, output.GetState().Has(PowerState.On));
        }

        [TestMethod]
        public void Should_TurnOn_IfButtonPressedShort()
        {
            var testController = new TestController();
            
            var automation = new TurnOnAndOffAutomation(
                "Test",
                testController.GetInstance<IDateTimeService>(),
                testController.GetInstance<ISchedulerService>(),
                testController.GetInstance<ISettingsService>(),
                testController.GetInstance<IDaylightService>(),
                testController.GetInstance<IMessageBrokerService>());

            var buttonAdapter = new TestButtonAdapter();
            var button = new Button("Test", buttonAdapter, testController.GetInstance<ITimerService>(), testController.GetInstance<ISettingsService>(), testController.GetInstance<IMessageBrokerService>(), testController.GetInstance<ILogService>());
            var output = new Lamp("Test", new TestLampAdapter());
            Assert.AreEqual(true, output.GetState().Has(PowerState.Off));

            automation.WithTrigger(button.CreatePressedShortTrigger(testController.GetInstance<IMessageBrokerService>()));
            automation.WithTarget(output);

            buttonAdapter.Touch();

            Assert.AreEqual(true, output.GetState().Has(PowerState.On));
        }

        [TestMethod]
        public void Should_NotTurnOn_IfMotionDetected_AndTimeRangeConditionIs_NotFulfilled()
        {
            var testController = new TestController();
            testController.SetTime(TimeSpan.Parse("18:00:00"));
            var adapter = new TestMotionDetectorAdapter();
            var motionDetector = new MotionDetector(
                "Test",
                adapter,
                testController.GetInstance<ISchedulerService>(),
                testController.GetInstance<ISettingsService>(),
                testController.GetInstance<IMessageBrokerService>());

            var automation = new TurnOnAndOffAutomation(
                "Test",
                testController.GetInstance<IDateTimeService>(),
                testController.GetInstance<ISchedulerService>(),
                testController.GetInstance<ISettingsService>(),
                testController.GetInstance<IDaylightService>(),
                testController.GetInstance<IMessageBrokerService>());

            var output = new Lamp("Test", new TestLampAdapter());
            Assert.AreEqual(true, output.GetState().Has(PowerState.Off));

            automation.WithTurnOnWithinTimeRange(() => TimeSpan.Parse("10:00:00"), () => TimeSpan.Parse("15:00:00"));
            automation.WithTrigger(motionDetector);
            automation.WithTarget(output);

            adapter.Invoke();

            Assert.AreEqual(true, output.GetState().Has(PowerState.Off));
        }

        [TestMethod]
        public void Should_TurnOn_IfButtonPressed_EvenIfTimeRangeConditionIs_NotFulfilled()
        {
            var testController = new TestController();
            testController.SetTime(TimeSpan.Parse("18:00:00"));

            var automation = new TurnOnAndOffAutomation(
                "Test",
                testController.GetInstance<IDateTimeService>(),
                testController.GetInstance<ISchedulerService>(),
                testController.GetInstance<ISettingsService>(),
                testController.GetInstance<IDaylightService>(),
                testController.GetInstance<IMessageBrokerService>());

            var buttonAdapter = new TestButtonAdapter();
            var button = new Button("Test", buttonAdapter, testController.GetInstance<ITimerService>(), testController.GetInstance<ISettingsService>(), testController.GetInstance<IMessageBrokerService>(), testController.GetInstance<ILogService>());
            var output = new Lamp("Test", new TestLampAdapter());
            Assert.AreEqual(true, output.GetState().Has(PowerState.Off));

            automation.WithTurnOnWithinTimeRange(() => TimeSpan.Parse("10:00:00"), () => TimeSpan.Parse("15:00:00"));
            automation.WithTrigger(button.CreatePressedShortTrigger(testController.GetInstance<IMessageBrokerService>()));
            automation.WithTarget(output);

            buttonAdapter.Touch();

            Assert.AreEqual(true, output.GetState().Has(PowerState.On));
        }

        [TestMethod]
        public void Should_NotTurnOn_IfMotionDetected_AndSkipConditionIs_Fulfilled()
        {
            var testController = new TestController();
            testController.SetTime(TimeSpan.Parse("14:00:00"));
            var adapter = new TestMotionDetectorAdapter();
            var motionDetector = new MotionDetector(
                "Test",
                adapter,
                testController.GetInstance<ISchedulerService>(),
                testController.GetInstance<ISettingsService>(),
                testController.GetInstance<IMessageBrokerService>());

            var automation = new TurnOnAndOffAutomation(
                "Test",
                testController.GetInstance<IDateTimeService>(),
                testController.GetInstance<ISchedulerService>(),
                testController.GetInstance<ISettingsService>(),
                testController.GetInstance<IDaylightService>(),
                testController.GetInstance<IMessageBrokerService>());

            var output = new Lamp("Test", new TestLampAdapter());
            Assert.AreEqual(true, output.GetState().Has(PowerState.Off));

            automation.WithTrigger(motionDetector);
            automation.WithTarget(output);

            var other2 = new Lamp("Test", new TestLampAdapter());
            other2.TryTurnOn();

            IComponent[] otherActuators =
            {
                new Lamp("Test", new TestLampAdapter()),
                other2
            };

            automation.WithSkipIfAnyIsAlreadyOn(otherActuators);

            adapter.Invoke();

            Assert.AreEqual(true, output.GetState().Has(PowerState.Off));
        }

        [TestMethod]
        public void Should_TurnOn_IfMotionDetected_AndSkipConditionIs_NotFulfilled()
        {
            var testController = new TestController();
            testController.SetTime(TimeSpan.Parse("14:00:00"));
            var adapter = new TestMotionDetectorAdapter();
            var motionDetector = new MotionDetector(
                "Test",
                adapter,
                testController.GetInstance<ISchedulerService>(),
                testController.GetInstance<ISettingsService>(),
                testController.GetInstance<IMessageBrokerService>());

            var automation = new TurnOnAndOffAutomation(
                "Test",
                testController.GetInstance<IDateTimeService>(),
                testController.GetInstance<ISchedulerService>(),
                testController.GetInstance<ISettingsService>(),
                testController.GetInstance<IDaylightService>(),
                testController.GetInstance<IMessageBrokerService>());

            var output = new Lamp("Test", new TestLampAdapter());
            Assert.AreEqual(true, output.GetState().Has(PowerState.Off));

            automation.WithTrigger(motionDetector);
            automation.WithTarget(output);

            IComponent[] otherActuators =
            {
                new Lamp("Test", new TestLampAdapter()),
                new Lamp("Test", new TestLampAdapter())
            };

            automation.WithSkipIfAnyIsAlreadyOn(otherActuators);

            adapter.Invoke();

            Assert.AreEqual(true, output.GetState().Has(PowerState.On));
        }

        [TestMethod]
        public void Should_TurnOff_IfButtonPressed_WhileTargetIsAlreadyOn()
        {
            var testController = new TestController();
            testController.SetTime(TimeSpan.Parse("14:00:00"));

            var automation = new TurnOnAndOffAutomation(
                "Test",
                testController.GetInstance<IDateTimeService>(),
                testController.GetInstance<ISchedulerService>(),
                testController.GetInstance<ISettingsService>(),
                testController.GetInstance<IDaylightService>(),
                testController.GetInstance<IMessageBrokerService>());

            var buttonAdapter = new TestButtonAdapter();
            var button = new Button("Test", buttonAdapter, testController.GetInstance<ITimerService>(), testController.GetInstance<ISettingsService>(), testController.GetInstance<IMessageBrokerService>(), testController.GetInstance<ILogService>());
            var output = new Lamp("Test", new TestLampAdapter());
            Assert.AreEqual(true, output.GetState().Has(PowerState.Off));

            automation.WithTrigger(button.CreatePressedShortTrigger(testController.GetInstance<IMessageBrokerService>()));
            automation.WithTarget(output);

            IComponent[] otherActuators =
            {
                new Lamp("Test", new TestLampAdapter()),
                new Lamp("Test", new TestLampAdapter())
            };

            automation.WithSkipIfAnyIsAlreadyOn(otherActuators);

            buttonAdapter.Touch();
            Assert.AreEqual(true, output.GetState().Has(PowerState.On));

            buttonAdapter.Touch();
            Assert.AreEqual(true, output.GetState().Has(PowerState.On));

            automation.WithTurnOffIfButtonPressedWhileAlreadyOn();
            buttonAdapter.Touch();
            Assert.AreEqual(true, output.GetState().Has(PowerState.Off));
        }
    }
}
