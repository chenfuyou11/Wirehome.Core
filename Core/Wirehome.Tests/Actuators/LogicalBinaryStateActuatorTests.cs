using Wirehome.Actuators.Lamps;
using Wirehome.Components;
using Wirehome.Contracts.Components.Commands;
using Wirehome.Contracts.Components.States;
using Wirehome.Tests.Mockups.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wirehome.Tests.Actuators
{
    [TestClass]
    public class LogicalBinaryStateActuatorTests
    {
        [TestMethod]
        public void CombinedComponent_TurnOnAndOff()
        {
            var lamp1 = new Lamp("Lamp1", new TestLampAdapter());
            var lamp2 = new Lamp("Lamp2", new TestLampAdapter());
            var lamp3 = new Lamp("Lamp3", new TestLampAdapter());

            var logicalComponent = new LogicalComponent("Test");
            logicalComponent.WithComponent(lamp1);
            logicalComponent.WithComponent(lamp2);
            logicalComponent.WithComponent(lamp3);

            logicalComponent.ExecuteCommand(new TurnOffCommand());
            Assert.IsTrue(logicalComponent.GetState().Has(PowerState.Off));
            Assert.IsTrue(lamp1.GetState().Has(PowerState.Off));
            Assert.IsTrue(lamp2.GetState().Has(PowerState.Off));
            Assert.IsTrue(lamp3.GetState().Has(PowerState.Off));

            logicalComponent.ExecuteCommand(new TurnOnCommand());
            Assert.IsTrue(logicalComponent.GetState().Has(PowerState.On));
            Assert.IsTrue(lamp1.GetState().Has(PowerState.On));
            Assert.IsTrue(lamp2.GetState().Has(PowerState.On));
            Assert.IsTrue(lamp3.GetState().Has(PowerState.On));

            logicalComponent.ExecuteCommand(new TurnOffCommand());
            Assert.IsTrue(logicalComponent.GetState().Has(PowerState.Off));
            Assert.IsTrue(lamp1.GetState().Has(PowerState.Off));
            Assert.IsTrue(lamp2.GetState().Has(PowerState.Off));
            Assert.IsTrue(lamp3.GetState().Has(PowerState.Off));
        }
    }
}
