using Wirehome.Contracts.Components.States;
using Wirehome.Contracts.Core;
using Wirehome.Contracts.Messaging;
using Wirehome.Contracts.Scheduling;
using Wirehome.Contracts.Settings;
using Wirehome.Sensors.MotionDetectors;
using Wirehome.Tests.Mockups;
using Wirehome.Tests.Mockups.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wirehome.Tests.Components
{
    [TestClass]
    public class MotionDetectorTests
    {
        [TestMethod]
        public void MotionDetector_Detect()
        {
            var adapter = new TestMotionDetectorAdapter();
            var motionDetector = CreateMotionDetector(adapter);

            Assert.IsTrue(motionDetector.GetState().Has(MotionDetectionState.Idle));

            adapter.Begin();
            Assert.IsTrue(motionDetector.GetState().Has(MotionDetectionState.MotionDetected));

            adapter.End();
            Assert.IsTrue(motionDetector.GetState().Has(MotionDetectionState.Idle));
        }

        [TestMethod]
        public void MotionDetector_DetectMultiple()
        {
            var adapter = new TestMotionDetectorAdapter();
            var motionDetector = CreateMotionDetector(adapter);

            Assert.IsTrue(motionDetector.GetState().Has(MotionDetectionState.Idle));

            adapter.Begin();
            Assert.IsTrue(motionDetector.GetState().Has(MotionDetectionState.MotionDetected));

            adapter.Begin();
            adapter.Begin();
            adapter.Begin();
            Assert.IsTrue(motionDetector.GetState().Has(MotionDetectionState.MotionDetected));
        }

        private MotionDetector CreateMotionDetector(TestMotionDetectorAdapter adapter)
        {
            var testController = new TestController();
            return new MotionDetector("Test", adapter, testController.GetInstance<ISchedulerService>(), testController.GetInstance<ISettingsService>(), testController.GetInstance<IMessageBrokerService>());
        }
    }
}
