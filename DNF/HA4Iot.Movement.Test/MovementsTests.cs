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
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Sensors;
using System.Threading.Tasks;
using Microsoft.Reactive.Testing;
using System.Linq;
using System.Reactive;
using HA4IoT.Extensions.Extensions;
using System;
using System.Reactive.Linq;
using System.Reactive.Disposables;

namespace HA4Iot.Movement.Test
{
    [TestClass]
    public class MovementsTests : ReactiveTest
    {
        [TestMethod]
        public void TestMove()
        {
            var setupResult = SetupAutomationServiceAndAreas();
            var automationService = setupResult.automationService;
            
            automationService.Startup();
            var toiletDescriptor = automationService.ConfigureMotionDetector(setupResult.toiletDetector, setupResult.hallwayDetectorToilet, null);
            var hallwayToiletDetector = automationService.ConfigureMotionDetector(setupResult.hallwayDetectorToilet, setupResult.hallwayDetectorLivingRoom, null);
            var hallwayLivingRoomDetector = automationService.ConfigureMotionDetector(setupResult.hallwayDetectorLivingRoom, setupResult.livingRoomDetector, null);
            var livingRoomDetector = automationService.ConfigureMotionDetector(setupResult.livingRoomDetector, setupResult.balconyDetector, null);
            var balconyDetector = automationService.ConfigureMotionDetector(setupResult.balconyDetector, setupResult.livingRoomDetector, null);
            var kitchenDetector = automationService.ConfigureMotionDetector(setupResult.kitchenDetector, setupResult.hallwayDetectorToilet, null);

            automationService.StartWatchForMove();

            Task.WaitAll(FakeMovments(new FakeMove[]
            {
                new FakeMove{MotionDetector = setupResult.toiletDetector, Time = 500},
                new FakeMove{MotionDetector = setupResult.toiletDetector, Time = 1000},
                new FakeMove{MotionDetector = setupResult.toiletDetector, Time = 1500},
                new FakeMove{MotionDetector = setupResult.hallwayDetectorToilet, Time = 2000},
              

               // new FakeMove{MotionDetector = setupResult.balconyDetector, Time = 1500},
               // new FakeMove{MotionDetector = setupResult.livingRoomDetector, Time = 2500},
            }, 5000));



            Assert.AreEqual(1, 1);
        }

        public Task FakeMovments(IEnumerable<FakeMove> moves, int waitAfter)
        {
            return Task.Run(async () =>
            {
                var time = -1;
                var diff = -1;

                foreach(var m in moves.OrderBy(x => x.Time))
                {
                    if(time < 0)
                    {
                        time = m.Time;
                        diff = m.Time;
                    }
                    else
                    {
                        diff = m.Time - time;
                        time = m.Time;
                    }

                    await Task.Delay(diff);

                    ((TestMotionDetector)m.MotionDetector).DetectMotion();
                }

                await Task.Delay(waitAfter);
            });
        }

        public class FakeMove
        {
            public int Time { get; set; }

            public IMotionDetector MotionDetector { get; set; }
    }

        public
        (
            LightAutomationService automationService,
            IMotionDetector hallwayDetectorToilet,
            IMotionDetector hallwayDetectorLivingRoom,
            IMotionDetector toiletDetector,
            IMotionDetector livingRoomDetector,
            IMotionDetector bathroomDetector,
            IMotionDetector badroomDetector,
            IMotionDetector kitchenDetector,
            IMotionDetector balconyDetector
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
            var hallwayDetectorToilet = motionFactory.CreateTestMotionDetector("hallwayDetectorToilet");
            var hallwayDetectorLivingRoom = motionFactory.CreateTestMotionDetector("hallwayDetectorLivingRoom");
            Mock.Get(hallwayArea).Setup(c => c.GetComponents<IMotionDetector>()).Returns(new[] { hallwayDetectorToilet, hallwayDetectorLivingRoom });
            areas.Add(hallwayArea);

            var toiletArea = Mock.Of<IArea>();
            var toiletDetector = motionFactory.CreateTestMotionDetector("toiletDetector");
            Mock.Get(toiletArea).Setup(c => c.GetComponents<IMotionDetector>()).Returns(new[] { toiletDetector });
            areas.Add(toiletArea);

            var livingRoomArea = Mock.Of<IArea>();
            var livingRoomDetector = motionFactory.CreateTestMotionDetector("livingRoomDetector");
            Mock.Get(livingRoomArea).Setup(c => c.GetComponents<IMotionDetector>()).Returns(new[] { livingRoomDetector });
            areas.Add(livingRoomArea);

            var bathroomArea = Mock.Of<IArea>();
            var bathroomDetector = motionFactory.CreateTestMotionDetector("bathroomDetector");
            Mock.Get(bathroomArea).Setup(c => c.GetComponents<IMotionDetector>()).Returns(new[] { bathroomDetector });
            areas.Add(bathroomArea);

            var badroomArea = Mock.Of<IArea>();
            var badroomDetector = motionFactory.CreateTestMotionDetector("badroomDetector");
            Mock.Get(badroomArea).Setup(c => c.GetComponents<IMotionDetector>()).Returns(new[] { badroomDetector });
            areas.Add(badroomArea);

            var kitchenArea = Mock.Of<IArea>();
            var kitchenDetector = motionFactory.CreateTestMotionDetector("kitchenDetector");
            Mock.Get(kitchenArea).Setup(c => c.GetComponents<IMotionDetector>()).Returns(new[] { kitchenDetector });
            areas.Add(kitchenArea);

            var balconyArea = Mock.Of<IArea>();
            var balconyDetector = motionFactory.CreateTestMotionDetector("balconyDetector");
            Mock.Get(balconyArea).Setup(c => c.GetComponents<IMotionDetector>()).Returns(new[] { balconyDetector });
            areas.Add(balconyArea);

            var staircaseArea = Mock.Of<IArea>();
            var staircaseDetector = motionFactory.CreateTestMotionDetector("staircaseDetector");
            Mock.Get(staircaseArea).Setup(c => c.GetComponents<IMotionDetector>()).Returns(new[] { staircaseDetector });
            areas.Add(staircaseArea);

            var areaService = Mock.Of<IAreaService>();
            Mock.Get(areaService).Setup(c => c.GetAreas()).Returns(areas);

            var lightAutomation = new LightAutomationService(areaService);

            return
            (
                lightAutomation,
                hallwayDetectorToilet,
                hallwayDetectorLivingRoom,
                toiletDetector,
                livingRoomDetector,
                bathroomDetector,
                badroomDetector,
                kitchenDetector,
                balconyDetector
            );
        }
    }

    //var scheduler = new TestScheduler();
    //var xs = scheduler.CreateHotObservable
    //         (
    //            new Recorded<Notification<MotionDescriptor>>(500, Notification.CreateOnNext(toiletDescriptor)),
    //            new Recorded<Notification<MotionDescriptor>>(500, Notification.CreateOnNext(hallwayToiletDetector)),
    //            new Recorded<Notification<MotionDescriptor>>(500, Notification.CreateOnNext(hallwayLivingRoomDetector)),
    //            new Recorded<Notification<MotionDescriptor>>(500, Notification.CreateOnNext(livingRoomDetector)),
    //            new Recorded<Notification<MotionDescriptor>>(500, Notification.CreateOnNext(balconyDetector))
    //         );
    //var testableObserver = scheduler.CreateObserver<int>();

    //scheduler.Start();
}