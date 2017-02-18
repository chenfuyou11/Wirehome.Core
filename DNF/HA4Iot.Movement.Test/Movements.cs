using Microsoft.VisualStudio.TestTools.UnitTesting;
using HA4IoT.Settings;
using HA4IoT.Services.Backup;
using HA4IoT.Services.Scheduling;
using HA4IoT.Services.System;
using HA4IoT.Extensions;
using HA4IoT.Contracts.Services.Storage;
using Moq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using HA4IoT.Services.Areas;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Sensors;

namespace UnitTestProject1
{
    [TestClass]
    public class Movements
    {
        [TestMethod]
        public void TestMove()
        {
            var setupResult = SetupAutomationServiceAndAreas();

            Assert.AreEqual(1, 1);
        }


        public 
        (
            LightAutomationService automationService,
            IMotionDetector hallwayDetectorToilet
        )
        SetupAutomationServiceAndAreas()
        {
            Dictionary<string, JObject> output;
            var storageService = Mock.Of<IStorageService>();
            Mock.Get(storageService).Setup(x => x.TryRead(It.IsAny<string>(), out output)).Returns(false);
            Mock.Get(storageService).Setup(x => x.Write(It.IsAny<string>(), It.IsAny<Dictionary<string, JObject>>()));

            var schedulerService = new SchedulerService(new TestTimerService(), new DateTimeService());
            var motionFactory = new TestMotionDetectorFactory(schedulerService, new SettingsService(new BackupService(), storageService));

            var areas = new List<IArea>();

            var hallwayArea = Mock.Of<IArea>();
            var hallwayDetectorToilet = motionFactory.CreateTestMotionDetector();
            var hallwayDetectorLivingRoom = motionFactory.CreateTestMotionDetector();
            Mock.Get(hallwayArea).Setup(c => c.GetComponents<IMotionDetector>()).Returns(new[] { hallwayDetectorToilet, hallwayDetectorLivingRoom });
            areas.Add(hallwayArea);

            var toiletArea = Mock.Of<IArea>();
            var toiletDetector = motionFactory.CreateTestMotionDetector();
            Mock.Get(toiletArea).Setup(c => c.GetComponents<IMotionDetector>()).Returns(new[] { toiletDetector });
            areas.Add(toiletArea);

            var livingRoomArea = Mock.Of<IArea>();
            var livingRoomDetector = motionFactory.CreateTestMotionDetector();
            Mock.Get(livingRoomArea).Setup(c => c.GetComponents<IMotionDetector>()).Returns(new[] { livingRoomDetector });
            areas.Add(livingRoomArea);

            var bathroomArea = Mock.Of<IArea>();
            var bathroomDetector = motionFactory.CreateTestMotionDetector();
            Mock.Get(bathroomArea).Setup(c => c.GetComponents<IMotionDetector>()).Returns(new[] { bathroomDetector });
            areas.Add(bathroomArea);

            var badroomArea = Mock.Of<IArea>();
            var badroomDetector = motionFactory.CreateTestMotionDetector();
            Mock.Get(badroomArea).Setup(c => c.GetComponents<IMotionDetector>()).Returns(new[] { badroomDetector });
            areas.Add(badroomArea);

            var kitchenArea = Mock.Of<IArea>();
            var kitchenDetector = motionFactory.CreateTestMotionDetector();
            Mock.Get(kitchenArea).Setup(c => c.GetComponents<IMotionDetector>()).Returns(new[] { kitchenDetector });
            areas.Add(kitchenArea);

            var balconyArea = Mock.Of<IArea>();
            var balconyDetector = motionFactory.CreateTestMotionDetector();
            Mock.Get(balconyArea).Setup(c => c.GetComponents<IMotionDetector>()).Returns(new[] { balconyDetector });
            areas.Add(balconyArea);

            var staircaseArea = Mock.Of<IArea>();
            var staircaseDetector = motionFactory.CreateTestMotionDetector();
            Mock.Get(staircaseArea).Setup(c => c.GetComponents<IMotionDetector>()).Returns(new[] { staircaseDetector });
            areas.Add(staircaseArea);

            var areaService = Mock.Of<IAreaService>();
            Mock.Get(areaService).Setup(c => c.GetAreas()).Returns(areas);

            return 
            (
                new LightAutomationService(areaService),
                hallwayDetectorToilet
            );
        }

    }
}
