using Wirehome.Actuators.Lamps;
using Wirehome.Components;
using Wirehome.Contracts.Components.States;
using Wirehome.Tests.Mockups.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wirehome.Tests.Components
{
    [TestClass]
    public class LampTests
    {
        [TestMethod]
        public void Lamp_Reset()
        {
            var adapter = new TestLampAdapter();
            var lamp = new Lamp("Test", adapter);
            lamp.ResetState();

            Assert.AreEqual(0, adapter.TurnOnCalledCount);
            Assert.AreEqual(1, adapter.TurnOffCalledCount);
            Assert.AreEqual(true, lamp.GetState().Has(PowerState.Off));
        }

        [TestMethod]
        public void Lamp_TurnOn()
        {
            var adapter = new TestLampAdapter();
            var lamp = new Lamp("Test", adapter);
            lamp.ResetState();

            Assert.AreEqual(0, adapter.TurnOnCalledCount);
            Assert.AreEqual(1, adapter.TurnOffCalledCount);
            Assert.AreEqual(true, lamp.GetState().Has(PowerState.Off));

            lamp.TryTurnOn();

            Assert.AreEqual(1, adapter.TurnOnCalledCount);
            Assert.AreEqual(1, adapter.TurnOffCalledCount);
            Assert.AreEqual(true, lamp.GetState().Has(PowerState.On));
        }

        [TestMethod]
        public void Lamp_Toggle()
        {
            var adapter = new TestLampAdapter();
            var lamp = new Lamp("Test", adapter);
            lamp.ResetState();

            Assert.AreEqual(0, adapter.TurnOnCalledCount);
            Assert.AreEqual(1, adapter.TurnOffCalledCount);
            Assert.AreEqual(true, lamp.GetState().Has(PowerState.Off));

            lamp.TryTogglePowerState();

            Assert.AreEqual(1, adapter.TurnOnCalledCount);
            Assert.AreEqual(1, adapter.TurnOffCalledCount);
            Assert.AreEqual(true, lamp.GetState().Has(PowerState.On));

            lamp.TryTogglePowerState();

            Assert.AreEqual(1, adapter.TurnOnCalledCount);
            Assert.AreEqual(2, adapter.TurnOffCalledCount);
            Assert.AreEqual(true, lamp.GetState().Has(PowerState.Off));
        }
    }
}
