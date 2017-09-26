using Wirehome.Actuators.Sockets;
using Wirehome.Components;
using Wirehome.Contracts.Components;
using Wirehome.Contracts.Components.States;
using Wirehome.Tests.Mockups;
using Wirehome.Tests.Mockups.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wirehome.Tests.Components
{
    [TestClass]
    public class SocketTests
    {
        [TestMethod]
        public void Socket_Reset()
        {
            var adapter = new TestBinaryOutputAdapter();
            var socket = new Socket("Test", adapter);
            socket.TryReset();

            Assert.AreEqual(0, adapter.TurnOnCalledCount);
            Assert.AreEqual(1, adapter.TurnOffCalledCount);
            Assert.IsTrue(socket.GetState().Has(PowerState.Off));
        }

        [TestMethod]
        public void Socket_TurnOnAndOff()
        {
            var adapter = new TestBinaryOutputAdapter();
            var socket = new Socket("Test", adapter);
            socket.TryReset();

            Assert.AreEqual(0, adapter.TurnOnCalledCount);
            Assert.AreEqual(1, adapter.TurnOffCalledCount);
            Assert.IsTrue(socket.GetState().Has(PowerState.Off));

            socket.TryTurnOn();

            Assert.AreEqual(1, adapter.TurnOnCalledCount);
            Assert.AreEqual(1, adapter.TurnOffCalledCount);
            Assert.IsTrue(socket.GetState().Has(PowerState.On));

            socket.TryTurnOn();

            Assert.AreEqual(1, adapter.TurnOnCalledCount);
            Assert.AreEqual(1, adapter.TurnOffCalledCount);
            Assert.IsTrue(socket.GetState().Has(PowerState.On));

            socket.TryTurnOff();

            Assert.AreEqual(1, adapter.TurnOnCalledCount);
            Assert.AreEqual(2, adapter.TurnOffCalledCount);
            Assert.IsTrue(socket.GetState().Has(PowerState.Off));

            socket.TryTurnOff();

            Assert.AreEqual(1, adapter.TurnOnCalledCount);
            Assert.AreEqual(2, adapter.TurnOffCalledCount);
            Assert.IsTrue(socket.GetState().Has(PowerState.Off));

            socket.TryTurnOn();

            Assert.AreEqual(2, adapter.TurnOnCalledCount);
            Assert.AreEqual(2, adapter.TurnOffCalledCount);
            Assert.IsTrue(socket.GetState().Has(PowerState.On));
        }
    }
}
