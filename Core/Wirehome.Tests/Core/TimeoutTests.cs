using System;
using Wirehome.Contracts.Core;
using Wirehome.Tests.Mockups;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wirehome.Core;

namespace Wirehome.Tests.Core
{
    [TestClass]
    public class TimeoutTests
    {
        [TestMethod]
        public void Timeout_Elapsed()
        {
            var testController = new TestController();
            var timeout = new Timeout(testController.GetInstance<ITimerService>());

            var eventFired = false;
            timeout.Elapsed += (s,e) => eventFired = true;

            Assert.IsFalse(timeout.IsEnabled);
            Assert.IsTrue(timeout.IsElapsed);
            Assert.IsFalse(eventFired);

            timeout.Start(TimeSpan.FromSeconds(2));

            Assert.IsTrue(timeout.IsEnabled);
            Assert.IsFalse(timeout.IsElapsed);
            Assert.IsFalse(eventFired);

            testController.Tick(TimeSpan.FromSeconds(1));

            Assert.IsTrue(timeout.IsEnabled);
            Assert.IsFalse(timeout.IsElapsed);
            Assert.IsFalse(eventFired);

            testController.Tick(TimeSpan.FromSeconds(1));

            Assert.IsFalse(timeout.IsEnabled);
            Assert.IsTrue(timeout.IsElapsed);
            Assert.IsTrue(eventFired);
        }
    }
}
