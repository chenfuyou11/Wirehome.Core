using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Wirehome.Contracts.Sensors;
using Wirehome.Extensions.MotionModel;
using Wirehome.Contracts.Logging;
using Wirehome.Contracts.Environment;
using Wirehome.Contracts.Scheduling;
using Wirehome.Contracts.Actuators;
using Wirehome.Extensions.Messaging.Core;
using Wirehome.Contracts.Core;
using System;
using System.Threading;

namespace Wirehome.Extensions.Tests
{
    //  ________________________________________<_    __________________________
    // |        |                |                       |                      |
    // |        |                         HALLWAY        |                      |
    // |   B    |                |<         [H]          |<                     |
    // |   A                     |___   ______           |       BADROOM        |
    // |   L    |                |            |                    [D]          |
    // |   C    |                |            |          |                      |
    // |   O    |                |            |          |______________________|
    // |   N    |   LIVINGROOM  >|            |          |<                     |
    // |   Y    |      [L]       |  BATHROOM  |                                 |
    // |        |                |     [B]   >|___v  ____|                      |
    // |  [Y]   |                |            |          |       KITCHEN        |
    // |        |                |            |  TOILET  |         [K]          |
    // |        |                |            |    [T]   |                      |
    // |_______v|________________|____________|_____v____|______________________|

    [TestClass]
    public class MovementsTests : ReactiveTest
    {
        [TestMethod]
        public void TestMove()
        {
            var (service, eventAggregator, scheduler) = SetupEnviroment();
            var motionEvents = scheduler.CreateColdObservable
            (
              OnNext(Time.Tics(500), new MotionEnvelope(ToiletId)),
              OnNext(Time.Tics(1000), new MotionEnvelope(KitchenId)),
              OnNext(Time.Tics(1500), new MotionEnvelope(HallwayToiletId)),
              OnNext(Time.Tics(2000), new MotionEnvelope(HallwayLivingroomId)),
              OnNext(Time.Tics(1300), new MotionEnvelope(BalconyId)),
              OnNext(Time.Tics(2000), new MotionEnvelope(LivingroomId))
            );
            Mock.Get(eventAggregator).Setup(x => x.Observe<MotionEvent>()).Returns(motionEvents);

            service.Initialize();

            var result = scheduler.Start(service.AnalyzeMove, 0, 0, long.MaxValue);

            Assert.AreEqual(1, 1);
        }

        #region Setup

        private const string HallwayToiletId = "HallwayToilet";
        private const string HallwayLivingroomId = "HallwayLivingroom";
        private const string ToiletId = "Toilet";
        private const string LivingroomId = "Livingroom";
        private const string BathroomId = "Bathroom";
        private const string BadroomId = "Badroom";
        private const string KitchenId = "Kitchen";
        private const string BalconyId = "Balcony";
        private const string StaircaseId = "Staircase";

        public
        (
            LightAutomationService service,
            IEventAggregator eventAggregator,
            TestScheduler scheduler
        )
        SetupEnviroment()
        {
            var hallwayDetectorToilet = CreateMotionDetector(HallwayToiletId);
            var hallwayDetectorLivingRoom = CreateMotionDetector(HallwayLivingroomId);
            var toiletDetector = CreateMotionDetector(ToiletId);
            var livingRoomDetector = CreateMotionDetector(LivingroomId);
            var bathroomDetector = CreateMotionDetector(BathroomId);
            var badroomDetector = CreateMotionDetector(BadroomId);
            var kitchenDetector = CreateMotionDetector(KitchenId);
            var balconyDetector = CreateMotionDetector(BalconyId);
            var staircaseDetector = CreateMotionDetector(StaircaseId);

            var schedulerService = Mock.Of<ISchedulerService>();
            var daylightService = Mock.Of<IDaylightService>();
            var logService = Mock.Of<ILogService>();
            var eventAggregator = Mock.Of<IEventAggregator>();
            var dateTimeService = Mock.Of<IDateTimeService>();
            var scheduler = new TestScheduler();
            var concurrencyProvider = new TestConcurrencyProvider(scheduler);
            var motionConfigurationProvider = new MotionConfigurationProvider();

            var lightAutomation = new LightAutomationService(eventAggregator, schedulerService, daylightService, logService, concurrencyProvider, dateTimeService,  motionConfigurationProvider);

            lightAutomation.RegisterDescriptor(toiletDetector.Id, new[] { hallwayDetectorToilet.Id }, Mock.Of<ILamp>());
            lightAutomation.RegisterDescriptor(hallwayDetectorToilet.Id, new[] { hallwayDetectorLivingRoom.Id, kitchenDetector.Id, staircaseDetector.Id }, Mock.Of<ILamp>());
            lightAutomation.RegisterDescriptor(hallwayDetectorLivingRoom.Id, new[] { livingRoomDetector.Id, bathroomDetector.Id, hallwayDetectorToilet.Id }, Mock.Of<ILamp>());
            lightAutomation.RegisterDescriptor(livingRoomDetector.Id, new[] { balconyDetector.Id }, Mock.Of<ILamp>());
            lightAutomation.RegisterDescriptor(balconyDetector.Id, new[] { livingRoomDetector.Id }, Mock.Of<ILamp>());
            lightAutomation.RegisterDescriptor(kitchenDetector.Id, new[] { hallwayDetectorToilet.Id }, Mock.Of<ILamp>());
            lightAutomation.RegisterDescriptor(bathroomDetector.Id, new[] { hallwayDetectorLivingRoom.Id }, Mock.Of<ILamp>());
            lightAutomation.RegisterDescriptor(badroomDetector.Id, new[] { hallwayDetectorLivingRoom.Id }, Mock.Of<ILamp>());
            lightAutomation.RegisterDescriptor(staircaseDetector.Id, new[] { hallwayDetectorToilet.Id }, Mock.Of<ILamp>());

            return
            (
                lightAutomation,
                eventAggregator,
                scheduler
            );
        }

        private IMotionDetector CreateMotionDetector(string id)
        {
            var mockDetector = Mock.Of<IMotionDetector>();
            Mock.Get(mockDetector).Setup(x => x.Id).Returns(id);
            return mockDetector;
        }
        #endregion
    }

    public class MotionEnvelope : MessageEnvelope<MotionEvent>
    {
        public MotionEnvelope(string motionUid) : base(new MotionEvent(motionUid))
        {
        }
    }
}
