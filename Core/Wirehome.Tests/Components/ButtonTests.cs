using System;
using Wirehome.Contracts.Components.Features;
using Wirehome.Contracts.Components.States;
using Wirehome.Contracts.Core;
using Wirehome.Contracts.Logging;
using Wirehome.Contracts.Messaging;
using Wirehome.Contracts.Settings;
using Wirehome.Sensors.Buttons;
using Wirehome.Tests.Mockups;
using Wirehome.Tests.Mockups.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wirehome.Tests.Components
{
    [TestClass]
    public class ButtonTests
    {
        [TestMethod]
        public void Button_FeatureAvailable()
        {
            var c = new TestController();

            var buttonAdapter = new TestButtonAdapter();
            var button = new Button("Test", buttonAdapter, c.GetInstance<ITimerService>(), c.GetInstance<ISettingsService>(), c.GetInstance<IMessageBrokerService>(), c.GetInstance<ILogService>());

            Assert.IsTrue(button.GetFeatures().Supports<ButtonFeature>());
        }

        [TestMethod]
        public void Button_StateAvailable()
        {
            var c = new TestController();

            var buttonAdapter = new TestButtonAdapter();
            var button = new Button("Test", buttonAdapter, c.GetInstance<ITimerService>(), c.GetInstance<ISettingsService>(), c.GetInstance<IMessageBrokerService>(), c.GetInstance<ILogService>());

            Assert.IsTrue(button.GetState().Supports<ButtonState>());
        }

        [TestMethod]
        public void Button_PressShort()
        {
            var c = new TestController();

            var buttonAdapter = new TestButtonAdapter();
            var button = new Button("Test", buttonAdapter, c.GetInstance<ITimerService>(), c.GetInstance<ISettingsService>(), c.GetInstance<IMessageBrokerService>(), c.GetInstance<ILogService>());

            Assert.IsTrue(button.GetState().Has(ButtonState.Released));
            buttonAdapter.Press();
            Assert.IsTrue(button.GetState().Has(ButtonState.Pressed));
            buttonAdapter.Release();
            Assert.IsTrue(button.GetState().Has(ButtonState.Released));
        }

        [TestMethod]
        public void Button_PressedShortTrigger()
        {
            var c = new TestController();

            var buttonAdapter = new TestButtonAdapter();
            var button = new Button("Test", buttonAdapter, c.GetInstance<ITimerService>(), c.GetInstance<ISettingsService>(), c.GetInstance<IMessageBrokerService>(), c.GetInstance<ILogService>());

            var triggerRaised = false;
            button.CreatePressedShortTrigger(c.GetInstance<IMessageBrokerService>()).Attach(() => triggerRaised = true);

            Assert.IsTrue(button.GetState().Has(ButtonState.Released));
            buttonAdapter.Touch();
            Assert.IsTrue(triggerRaised);
        }

        [TestMethod]
        public void Button_PressedLongTrigger()
        {
            var c = new TestController();

            var buttonAdapter = new TestButtonAdapter();
            var button = new Button("Test", buttonAdapter, c.GetInstance<ITimerService>(), c.GetInstance<ISettingsService>(), c.GetInstance<IMessageBrokerService>(), c.GetInstance<ILogService>());

            var triggerRaised = false;
            button.CreatePressedLongTrigger(c.GetInstance<IMessageBrokerService>()).Attach(() => triggerRaised = true);

            Assert.IsTrue(button.GetState().Has(ButtonState.Released));
            buttonAdapter.Press();

            // Should be false because time was too slow.
            Assert.IsFalse(triggerRaised);

            // 1h is only for test. Enough to test with default settings.
            c.Tick(TimeSpan.FromHours(1));
            Assert.IsTrue(triggerRaised);
        }

        [TestMethod]
        public void Button_NoDoublePressedLongTrigger()
        {
            var c = new TestController();

            var buttonAdapter = new TestButtonAdapter();
            var button = new Button("Test", buttonAdapter, c.GetInstance<ITimerService>(), c.GetInstance<ISettingsService>(), c.GetInstance<IMessageBrokerService>(), c.GetInstance<ILogService>());

            var triggerRaisedCount = 0;
            button.CreatePressedLongTrigger(c.GetInstance<IMessageBrokerService>()).Attach(() => triggerRaisedCount++);

            buttonAdapter.Press();

            // 1h is only for test. Enough to test with default settings.
            c.Tick(TimeSpan.FromHours(1));
            
            Assert.AreEqual(1, triggerRaisedCount);

            buttonAdapter.Release();

            Assert.AreEqual(1, triggerRaisedCount);
        }
    }
}
