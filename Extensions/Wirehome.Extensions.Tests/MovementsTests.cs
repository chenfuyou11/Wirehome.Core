using Moq;
using System;
using System.Reactive.Linq;
using System.Collections.Generic;
using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wirehome.Contracts.Sensors;
using Wirehome.Contracts.Logging;
using Wirehome.Contracts.Environment;
using Wirehome.Extensions.Messaging.Core;
using Wirehome.Contracts.Core;
using Force.DeepCloner;
using System.Reactive;
using Wirehome.Motion.Model;
using Wirehome.Motion;

namespace Wirehome.Extensions.Tests
{
    //                                             STAIRCASE [S]
    //  ________________________________________<_    __________________________
    // |        |                |                       |                      |
    // |        |                  [HL]   HALLWAY        |                      |
    // |   B    |                |<         [H]          |<                     |
    // |   A                     |___   ______           |       BADROOM        |
    // |   L    |                |            |                    [D]          |
    // |   C    |                |            |          |                      |
    // |   O    |                |            |          |______________________|
    // |   N    |   LIVINGROOM  >|            |          |<                     |
    // |   Y    |      [L]       |  BATHROOM  |   [HT]                          |
    // |        |                |     [B]   >|___v  ____|                      |
    // |  [Y]   |                |            |          |       KITCHEN        |
    // |        |                |            |  TOILET  |         [K]          |
    // |        |                |            |    [T]   |                      |
    // |_______v|________________|____________|_____v____|______________________|
    //
    // LEGEND: v/< - Motion Detector

    [TestClass]
    public class MovementsTests : ReactiveTest
    {
        [TestMethod]
        public void MoveInRoomShouldTurnOnLight()
        {
            var (service, motionEvents, scheduler, lampDictionary, dateTime) = SetupEnviroment(null,
              OnNext(Time.Tics(500), new MotionEnvelope(ToiletId)),
              OnNext(Time.Tics(1500), new MotionEnvelope(KitchenId)),
              OnNext(Time.Tics(2000), new MotionEnvelope(LivingroomId))
            );

            service.Start();
            scheduler.AdvanceToEnd(motionEvents);
            
            Assert.AreEqual(true, lampDictionary[ToiletId].IsTurnedOn);
            Assert.AreEqual(true, lampDictionary[KitchenId].IsTurnedOn);
            Assert.AreEqual(true, lampDictionary[LivingroomId].IsTurnedOn);
        }

        [TestMethod]
        public void MoveInRoomShouldTurnOnLightOnWhenWorkinghoursAreDaylight()
        {
            var (service, motionEvents, scheduler, lampDictionary, dateTime) = SetupEnviroment(new AreaDescriptor { WorkingTime = WorkingTime.DayLight },
              OnNext(Time.Tics(500), new MotionEnvelope(ToiletId)),
              OnNext(Time.Tics(1500), new MotionEnvelope(KitchenId)),
              OnNext(Time.Tics(2000), new MotionEnvelope(LivingroomId))
            );

            Mock.Get(dateTime).Setup(x => x.Time).Returns(TimeSpan.FromHours(12));

            service.Start();
            scheduler.AdvanceToEnd(motionEvents);

            Assert.AreEqual(true, lampDictionary[ToiletId].IsTurnedOn);
            Assert.AreEqual(true, lampDictionary[KitchenId].IsTurnedOn);
            Assert.AreEqual(true, lampDictionary[LivingroomId].IsTurnedOn);
        }

        [TestMethod]
        public void MoveInRoomShouldNotTurnOnLightOnNightWhenWorkinghoursAreDaylight()
        {
            var (service, motionEvents, scheduler, lampDictionary, dateTime) = SetupEnviroment(new AreaDescriptor { WorkingTime = WorkingTime.DayLight },
              OnNext(Time.Tics(500), new MotionEnvelope(ToiletId)),
              OnNext(Time.Tics(1500), new MotionEnvelope(KitchenId)),
              OnNext(Time.Tics(2000), new MotionEnvelope(LivingroomId))
            );

            Mock.Get(dateTime).Setup(x => x.Time).Returns(TimeSpan.FromHours(21));

            service.Start();
            scheduler.AdvanceToEnd(motionEvents);

            Assert.AreEqual(false, lampDictionary[ToiletId].IsTurnedOn);
            Assert.AreEqual(false, lampDictionary[KitchenId].IsTurnedOn);
            Assert.AreEqual(false, lampDictionary[LivingroomId].IsTurnedOn);
        }

        [TestMethod]
        public void MoveInRoomShouldNotTurnOnLightOnDaylightWhenWorkinghoursIsNight()
        {
            var (service, motionEvents, scheduler, lampDictionary, dateTime) = SetupEnviroment(new AreaDescriptor { WorkingTime = WorkingTime.AfterDusk },
              OnNext(Time.Tics(500), new MotionEnvelope(ToiletId)),
              OnNext(Time.Tics(1500), new MotionEnvelope(KitchenId)),
              OnNext(Time.Tics(2000), new MotionEnvelope(LivingroomId))
            );
            Mock.Get(dateTime).Setup(x => x.Time).Returns(TimeSpan.FromHours(12));

            service.Start();
            scheduler.AdvanceToEnd(motionEvents);

            Assert.AreEqual(false, lampDictionary[ToiletId].IsTurnedOn);
            Assert.AreEqual(false, lampDictionary[KitchenId].IsTurnedOn);
            Assert.AreEqual(false, lampDictionary[LivingroomId].IsTurnedOn);
        }

        [TestMethod]
        public void MoveInRoomShouldNotTurnOnLightWhenAutomationIsDisabled()
        {
            var (service, motionEvents, scheduler, lampDictionary, dateTime) = SetupEnviroment(null,
              OnNext(Time.Tics(500), new MotionEnvelope(ToiletId))
            );

            service.DisableAutomation(ToiletId);
            service.Start();
            scheduler.AdvanceToEnd(motionEvents);

            Assert.AreEqual(false, lampDictionary[ToiletId].IsTurnedOn);
        }

        [TestMethod]
        public void MoveInRoomShouldTurnOnLightWhenAutomationIsReEnabled()
        {
            var (service, motionEvents, scheduler, lampDictionary, dateTime) = SetupEnviroment(null,
              OnNext(Time.Tics(500), new MotionEnvelope(ToiletId)),
              OnNext(Time.Tics(2500), new MotionEnvelope(ToiletId))
            );

            service.DisableAutomation(ToiletId);
            motionEvents.Subscribe(x => service.EnableAutomation(ToiletId));
            service.Start();
            scheduler.AdvanceToEnd(motionEvents);

            Assert.AreEqual(true, lampDictionary[ToiletId].IsTurnedOn);
        }

        [TestMethod]
        public void AnalyzeMoveShouldCountPeopleNumberInRoom()
        {
            var (service, motionEvents, scheduler, lampDictionary, dateTime) = SetupEnviroment(null,
              OnNext(Time.Tics(500), new MotionEnvelope(ToiletId)),
              OnNext(Time.Tics(1500), new MotionEnvelope(HallwayToiletId)),
              OnNext(Time.Tics(2000), new MotionEnvelope(KitchenId)),
              OnNext(Time.Tics(2500), new MotionEnvelope(LivingroomId)),
              OnNext(Time.Tics(3000), new MotionEnvelope(HallwayLivingroomId)),
              OnNext(Time.Tics(3500), new MotionEnvelope(HallwayToiletId)),
              OnNext(Time.Tics(4000), new MotionEnvelope(KitchenId))

            );

            service.Start();
            scheduler.AdvanceTo(service.Configuration.ConfusionResolutionTime + TimeSpan.FromMilliseconds(6000));

            Assert.AreEqual(2, service.GetCurrentNumberOfPeople(KitchenId));
        }

        [TestMethod]
        public void AnalyzeMoveShouldCountPeopleNumberInHouse()
        {
            var (service, motionEvents, scheduler, lampDictionary, dateTime) = SetupEnviroment(null,
              OnNext(Time.Tics(500), new MotionEnvelope(ToiletId)),
              OnNext(Time.Tics(1500), new MotionEnvelope(HallwayToiletId)),
              OnNext(Time.Tics(2000), new MotionEnvelope(KitchenId)),
              OnNext(Time.Tics(2500), new MotionEnvelope(LivingroomId)),
              OnNext(Time.Tics(3000), new MotionEnvelope(HallwayLivingroomId)),
              OnNext(Time.Tics(3500), new MotionEnvelope(HallwayToiletId)),
              OnNext(Time.Tics(4000), new MotionEnvelope(KitchenId))

            );

            service.Start();
            scheduler.AdvanceJustAfterEnd(motionEvents);

            Assert.AreEqual(2, service.NumberOfPersonsInHouse);
        }

        [TestMethod]
        public void WhenLeaveFromOnePersonRoomWithNoConfusionShouldTurnOffLightImmediately()
        {
            var (service, motionEvents, scheduler, lampDictionary, dateTime) = SetupEnviroment(null,
              OnNext(Time.Tics(500), new MotionEnvelope(ToiletId)),
              OnNext(Time.Tics(1500), new MotionEnvelope(HallwayToiletId))
            );

            service.Start();
            scheduler.AdvanceJustAfterEnd(motionEvents);

            Assert.AreEqual(false, lampDictionary[ToiletId].IsTurnedOn);
        }


        [TestMethod]
        public void WhenLeaveFromOnePersonRoomWithConfusionShouldTurnOffWhenConfusionResolved()
        {
            
            var (service, motionEvents, scheduler, lampDictionary, dateTime) = SetupEnviroment(null,
                  OnNext(Time.Tics(500), new MotionEnvelope(ToiletId)),
                  OnNext(Time.Tics(1000), new MotionEnvelope(KitchenId)),
                  OnNext(Time.Tics(1500), new MotionEnvelope(HallwayToiletId)),
                  OnNext(Time.Tics(2000), new MotionEnvelope(HallwayLivingroomId)),
                  OnNext(Time.Tics(3000), new MotionEnvelope(KitchenId))
            );

            service.Start();
            scheduler.AdvanceTo(service.Configuration.ConfusionResolutionTime + TimeSpan.FromMilliseconds(1500));

            Assert.AreEqual(false, lampDictionary[ToiletId].IsTurnedOn);
        }


        [TestMethod]
        public void WhenLeaveFromRoomWithNoConfusionShouldTurnOffLightAfterSomeTime()
        {
            var (service, motionEvents, scheduler, lampDictionary, dateTime) = SetupEnviroment(null,
              OnNext(Time.Tics(500), new MotionEnvelope(KitchenId)),
              OnNext(Time.Tics(1500), new MotionEnvelope(HallwayToiletId))
            );
            
            service.Start();

            scheduler.AdvanceJustAfterEnd(motionEvents);
            Assert.AreEqual(true, lampDictionary[KitchenId].IsTurnedOn);
            scheduler.AdvanceTo(Time.Tics(2500));
            Assert.AreEqual(false, lampDictionary[KitchenId].IsTurnedOn);
        }


        [TestMethod]
        public void WhenNoMoveInRoomShouldTurnOffAfterTurnOffTimeout()
        {
            var (service, motionEvents, scheduler, lampDictionary, dateTime) = SetupEnviroment(null,
                OnNext(Time.Tics(500), new MotionEnvelope(KitchenId))
            );

            service.Start();
            var area = service.GetAreaDescriptor(KitchenId);

            scheduler.AdvanceJustAfterEnd(motionEvents);
            Assert.AreEqual(true, lampDictionary[KitchenId].IsTurnedOn);
            scheduler.AdvanceTo(area.TurnOffTimeout);
            Assert.AreEqual(true, lampDictionary[KitchenId].IsTurnedOn);
            scheduler.AdvanceJustAfter(area.TurnOffTimeout);
            Assert.AreEqual(false, lampDictionary[KitchenId].IsTurnedOn);
        }

        /// <summary>
        /// First not confused moving path should not be source of confusion for next path
        /// HT -> HL is eliminated because of that
        /// T -> HT -> K | L -> HL -> HT -> K
        /// </summary>
        [TestMethod]
        public void MoveInFirstPathShouldNotConfusedNextPathWhenItIsSure()
        {
            var (service, motionEvents, scheduler, lampDictionary, dateTime) = SetupEnviroment(null,
                // First path
                OnNext(Time.Tics(500), new MotionEnvelope(ToiletId)),
                OnNext(Time.Tics(1500), new MotionEnvelope(HallwayToiletId)),
                OnNext(Time.Tics(2000), new MotionEnvelope(KitchenId)),
                // Second path
                OnNext(Time.Tics(2500), new MotionEnvelope(LivingroomId)),
                OnNext(Time.Tics(3000), new MotionEnvelope(HallwayLivingroomId))
            );

            service.Start();
            
            scheduler.AdvanceJustAfterEnd(motionEvents);

            Assert.AreEqual(0, service.NumberOfConfusions);
            Assert.AreEqual(true, lampDictionary[KitchenId].IsTurnedOn);
        }


        [TestMethod]
        public void CrossPass()
        {
            var (service, motionEvents, scheduler, lampDictionary, dateTime) = SetupEnviroment(null,
                
                OnNext(Time.Tics(500), new MotionEnvelope(KitchenId)),
                OnNext(Time.Tics(501), new MotionEnvelope(LivingroomId)),

                OnNext(Time.Tics(1500), new MotionEnvelope(HallwayToiletId)),
                OnNext(Time.Tics(1501), new MotionEnvelope(HallwayLivingroomId)),

                //OnNext(Time.Tics(2000), new MotionEnvelope(HallwayLivingroomId)), <- Undetected due motion detectors lag after previous move
                //OnNext(Time.Tics(2000), new MotionEnvelope(HallwayToiletId)), <- Undetected due motion detectors lag after previous move

                OnNext(Time.Tics(3000), new MotionEnvelope(LivingroomId)),
                OnNext(Time.Tics(3001), new MotionEnvelope(KitchenId))
            );

            service.Start();

            scheduler.AdvanceJustAfter(TimeSpan.FromMilliseconds(501));
            scheduler.AdvanceJustAfter(TimeSpan.FromMilliseconds(1501));
            scheduler.AdvanceJustAfter(TimeSpan.FromMilliseconds(3001));
            
            
            //scheduler.AdvanceJustAfterEnd(motionEvents);

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
        private const int TIMER_DURATION = 20;

        public
        (
            LightAutomationService,
            ITestableObservable<MotionEnvelope>,
            TestScheduler,
            Dictionary<string, MotionLamp>,
            IDateTimeService
        )
        SetupEnviroment(AreaDescriptor areaDescription = null, params Recorded<Notification<MotionEnvelope>>[] messages)
        {
            AreaDescriptor area = areaDescription ?? new AreaDescriptor();
            
            var hallwayDetectorToilet = CreateMotionDetector(HallwayToiletId);
            var hallwayDetectorLivingRoom = CreateMotionDetector(HallwayLivingroomId);
            var toiletDetector = CreateMotionDetector(ToiletId);
            var livingRoomDetector = CreateMotionDetector(LivingroomId);
            var bathroomDetector = CreateMotionDetector(BathroomId);
            var badroomDetector = CreateMotionDetector(BadroomId);
            var kitchenDetector = CreateMotionDetector(KitchenId);
            var balconyDetector = CreateMotionDetector(BalconyId);
            var staircaseDetector = CreateMotionDetector(StaircaseId);

            var hallwayLampToilet = new MotionLamp(HallwayToiletId);
            var hallwayLampLivingRoom = new MotionLamp(HallwayLivingroomId);
            var toiletLamp = new MotionLamp(ToiletId);
            var livingRoomLamp = new MotionLamp(LivingroomId);
            var bathroomLamp = new MotionLamp(BathroomId);
            var badroomLamp = new MotionLamp(BadroomId);
            var kitchenLamp = new MotionLamp(KitchenId);
            var balconyLamp = new MotionLamp(BadroomId);
            var staircaseLamp = new MotionLamp(StaircaseId);

            var lampDictionary = new Dictionary<string, MotionLamp>
            {
                { HallwayToiletId, hallwayLampToilet },
                { HallwayLivingroomId, hallwayLampLivingRoom },
                { ToiletId, toiletLamp },
                { LivingroomId, livingRoomLamp },
                { BathroomId, bathroomLamp },
                { BadroomId, badroomLamp },
                { KitchenId, kitchenLamp },
                { BalconyId, balconyLamp },
                { StaircaseId, staircaseLamp }
            };
        
            var daylightService = Mock.Of<IDaylightService>();
            Mock.Get(daylightService).Setup(x => x.Sunrise).Returns(TimeSpan.FromHours(8));
            Mock.Get(daylightService).Setup(x => x.Sunset).Returns(TimeSpan.FromHours(20));

            var logService = Mock.Of<ILogService>();
            var logger = Mock.Of<ILogger>();
            Mock.Get(logService).Setup(x => x.CreatePublisher(It.IsAny<string>())).Returns(logger);
            Mock.Get(logger).Setup(x => x.Info(It.IsAny<string>())).Callback((string message) => Console.WriteLine(message));
            Mock.Get(logger).Setup(x => x.Error(It.IsAny<string>())).Callback((string message) => Console.WriteLine(message));
            Mock.Get(logger).Setup(x => x.Warning(It.IsAny<string>())).Callback((string message) => Console.WriteLine(message));



            var eventAggregator = Mock.Of<IEventAggregator>();
            var dateTimeService = Mock.Of<IDateTimeService>();
            var scheduler = new TestScheduler();
            var concurrencyProvider = new TestConcurrencyProvider(scheduler);
            var motionConfigurationProvider = new MotionConfigurationProvider();
            var motionConfiguration = motionConfigurationProvider.GetConfiguration();

            var observableTimer = Mock.Of<IObservableTimer>();

            Mock.Get(observableTimer).Setup(x => x.GenerateTime(motionConfiguration.PeriodicCheckTime)).Returns(scheduler.CreateColdObservable(GenerateTestTime(TimeSpan.FromSeconds(TIMER_DURATION), motionConfiguration.PeriodicCheckTime)));

            var lightAutomation = new LightAutomationService(eventAggregator, daylightService, logService, concurrencyProvider, dateTimeService,  motionConfigurationProvider, observableTimer);

            var descriptors = new List<RoomInitializer>
            {
                new RoomInitializer(hallwayDetectorToilet.Id, new[] { hallwayDetectorLivingRoom.Id, kitchenDetector.Id, staircaseDetector.Id, toiletDetector.Id }, hallwayLampToilet, area.DeepClone()),
                new RoomInitializer(hallwayDetectorLivingRoom.Id, new[] { livingRoomDetector.Id, bathroomDetector.Id, hallwayDetectorToilet.Id }, hallwayLampLivingRoom, area.DeepClone()),
                new RoomInitializer(livingRoomDetector.Id, new[] { balconyDetector.Id, hallwayDetectorLivingRoom.Id }, livingRoomLamp, area.DeepClone()),
                new RoomInitializer(balconyDetector.Id, new[] { livingRoomDetector.Id }, balconyLamp, area.DeepClone()),
                new RoomInitializer(kitchenDetector.Id, new[] { hallwayDetectorToilet.Id }, kitchenLamp, area.DeepClone()),
                new RoomInitializer(bathroomDetector.Id, new[] { hallwayDetectorLivingRoom.Id }, bathroomLamp, area.DeepClone()),
                new RoomInitializer(badroomDetector.Id, new[] { hallwayDetectorLivingRoom.Id }, badroomLamp, area.DeepClone()),
                new RoomInitializer(staircaseDetector.Id, new[] { hallwayDetectorToilet.Id }, staircaseLamp, area.DeepClone()),
            };

            var toiletArea = area.DeepClone();
            toiletArea.MaxPersonCapacity = 1;
            descriptors.Add(new RoomInitializer(toiletDetector.Id, new[] { hallwayDetectorToilet.Id }, toiletLamp, toiletArea));

            lightAutomation.RegisterRooms(descriptors);
            lightAutomation.Initialize();

            var motionEvents = scheduler.CreateColdObservable(messages);
            Mock.Get(eventAggregator).Setup(x => x.Observe<MotionEvent>()).Returns(motionEvents);

            return
            (
                lightAutomation,
                motionEvents,
                scheduler,
                lampDictionary,
                dateTimeService
            );
        }

        public Recorded<Notification<DateTimeOffset>>[] GenerateTestTime(TimeSpan duration, TimeSpan frequency)
        {
            var time = new List<Recorded<Notification<DateTimeOffset>>>();
            var durationSoFar = TimeSpan.FromTicks(0);
            var dateSoFar = new DateTimeOffset(1, 1, 1, 0, 0, 0, TimeSpan.FromTicks(0));
            while (true)
            {
                durationSoFar = durationSoFar.Add(frequency);
                if (durationSoFar > duration) break;

                dateSoFar = dateSoFar.Add(frequency);
                time.Add(new Recorded<Notification<DateTimeOffset>>(durationSoFar.Ticks, Notification.CreateOnNext(dateSoFar)));
            }

            return time.ToArray();
        }

        private IMotionDetector CreateMotionDetector(string id)
        {
            var mockDetector = Mock.Of<IMotionDetector>();
            Mock.Get(mockDetector).Setup(x => x.Id).Returns(id);
            return mockDetector;
        }

        public class MotionEnvelope : MessageEnvelope<MotionEvent>
        {
            public MotionEnvelope(string motionUid) : base(new MotionEvent(motionUid))
            {
            }
        }

        
   
        #endregion
    }
}
