using Wirehome.Actuators.Lamps;
using Wirehome.Components;
using Wirehome.Contracts.Components.States;
using Wirehome.Contracts.Core;
using Wirehome.Contracts.Logging;
using Wirehome.Contracts.Messaging;
using Wirehome.Contracts.Settings;
using Wirehome.Contracts.Triggers;
using Wirehome.Sensors.Buttons;
using Wirehome.Sensors.TemperatureSensors;
using Wirehome.Tests.Mockups;
using Wirehome.Tests.Mockups.Adapters;
using Wirehome.Triggers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wirehome.Tests.Actuators
{
    [TestClass]
    public class TriggerTests
    {
        [TestMethod]
        public void Trigger_Invoke()
        {
            var attachTriggered = false;
            var eventTriggered = false;

            var trigger = new Trigger();
            trigger.Attach(() => attachTriggered = true);
            Assert.AreEqual(true, trigger.IsAnyAttached);

            trigger.Triggered += (s, e) => eventTriggered = true;
            Assert.AreEqual(true, trigger.IsAnyAttached);

            trigger.Execute();

            Assert.AreEqual(true, attachTriggered);
            Assert.AreEqual(true, eventTriggered);
        }

        [TestMethod]
        public void Trigger_SensorValueReached()
        {
            var testController = new TestController();

            var adapter = new TestNumericSensorAdapter();
            var sensor = new TemperatureSensor(
                "Test",
                adapter,
                testController.GetInstance<ISettingsService>());

            var trigger = sensor.GetTemperatureReachedTrigger(10.2F, 3.0F);

            var triggerCount = 0;
            trigger.Attach(() => triggerCount++);

            adapter.UpdateValue(5);
            Assert.AreEqual(0, triggerCount);

            adapter.UpdateValue(10);
            Assert.AreEqual(0, triggerCount);

            adapter.UpdateValue(10.2F);
            Assert.AreEqual(1, triggerCount);

            adapter.UpdateValue(9.0F);
            Assert.AreEqual(1, triggerCount);

            adapter.UpdateValue(13.0F);
            Assert.AreEqual(1, triggerCount);

            adapter.UpdateValue(5.0F);
            Assert.AreEqual(1, triggerCount);

            adapter.UpdateValue(10.2F);
            Assert.AreEqual(2, triggerCount);
        }

        [TestMethod]
        public void Trigger_SensorValueUnderran()
        {
            var testController = new TestController();

            var adapter = new TestNumericSensorAdapter();
            var sensor = new TemperatureSensor(
                "Test",
                adapter,
                testController.GetInstance<ISettingsService>());

            var trigger = sensor.GetTemperatureUnderranTrigger(10F, 3F);

            var triggerCount = 0;
            trigger.Attach(() => triggerCount++);

            adapter.UpdateValue(5);
            Assert.AreEqual(1, triggerCount);

            adapter.UpdateValue(10);
            Assert.AreEqual(1, triggerCount);

            adapter.UpdateValue(13.1F);
            Assert.AreEqual(1, triggerCount);

            adapter.UpdateValue(9F);
            Assert.AreEqual(2, triggerCount);

            adapter.UpdateValue(13.0F);
            Assert.AreEqual(2, triggerCount);

            adapter.UpdateValue(5F);
            Assert.AreEqual(2, triggerCount);

            adapter.UpdateValue(13.1F);
            Assert.AreEqual(2, triggerCount);

            adapter.UpdateValue(9.9F);
            Assert.AreEqual(3, triggerCount);
        }

        [TestMethod]
        public void Trigger_AttachAction()
        {
            var c = new TestController();
            
            var buttonAdapter = new TestButtonAdapter();
            var button = new Button("Test", buttonAdapter, c.GetInstance<ITimerService>(), c.GetInstance<ISettingsService>(), c.GetInstance<IMessageBrokerService>(), c.GetInstance<ILogService>());
            var lamp = new Lamp("Test", new TestLampAdapter());

            button.CreatePressedShortTrigger(c.GetInstance<IMessageBrokerService>()).Attach(() => lamp.TryTogglePowerState());

            lamp.GetState().Has(PowerState.Off);
            buttonAdapter.Touch();
            lamp.GetState().Has(PowerState.On);
            buttonAdapter.Touch();
            lamp.GetState().Has(PowerState.Off);
        }
    }
}
