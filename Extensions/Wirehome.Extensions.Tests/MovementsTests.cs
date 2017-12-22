using Moq;
using System;
using System.Collections.Generic;
using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wirehome.Contracts.Sensors;
using Wirehome.Extensions.MotionModel;
using Wirehome.Contracts.Logging;
using Wirehome.Contracts.Environment;
using Wirehome.Contracts.Scheduling;
using Wirehome.Extensions.Messaging.Core;
using Wirehome.Contracts.Core;
using Wirehome.Contracts.Components;
using Wirehome.Contracts.Components.Commands;

namespace Wirehome.Extensions.Tests
{
    //                                             STAIRCASE [S]
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
    //
    // LEGEND: v/< - Motion Detector

    [TestClass]
    public class MovementsTests : ReactiveTest
    {
        [TestMethod]
        public void MoveInRoomShouldTurnOnLight()
        {
            var (service, eventAggregator, scheduler, lampDictionary, dateTime) = SetupEnviroment();
            var motionEvents = scheduler.CreateColdObservable
            (
              OnNext(Time.Tics(500), new MotionEnvelope(ToiletId)),
              OnNext(Time.Tics(1500), new MotionEnvelope(KitchenId)),
              OnNext(Time.Tics(2000), new MotionEnvelope(LivingroomId))
            );
            Mock.Get(eventAggregator).Setup(x => x.Observe<MotionEvent>()).Returns(motionEvents);

            service.Initialize();

            var result = scheduler.Start(service.AnalyzeMove, 0, 0, long.MaxValue);

            Assert.AreEqual(true, lampDictionary[ToiletId].IsTurnedOn);
            Assert.AreEqual(true, lampDictionary[KitchenId].IsTurnedOn);
            Assert.AreEqual(true, lampDictionary[LivingroomId].IsTurnedOn);
        }

        [TestMethod]
        public void MoveInRoomShouldTurnOnLightOnWhenWorkinghoursAreDaylight()
        {
            var (service, eventAggregator, scheduler, lampDictionary, dateTime) = SetupEnviroment(new AreaDescriptor { WorkingTime = WorkingTime.DayLight });
            var motionEvents = scheduler.CreateColdObservable
            (
              OnNext(Time.Tics(500), new MotionEnvelope(ToiletId)),
              OnNext(Time.Tics(1500), new MotionEnvelope(KitchenId)),
              OnNext(Time.Tics(2000), new MotionEnvelope(LivingroomId))
            );
            Mock.Get(eventAggregator).Setup(x => x.Observe<MotionEvent>()).Returns(motionEvents);
            Mock.Get(dateTime).Setup(x => x.Time).Returns(TimeSpan.FromHours(12));

            service.Initialize();

            var result = scheduler.Start(service.AnalyzeMove, 0, 0, long.MaxValue);

            Assert.AreEqual(true, lampDictionary[ToiletId].IsTurnedOn);
            Assert.AreEqual(true, lampDictionary[KitchenId].IsTurnedOn);
            Assert.AreEqual(true, lampDictionary[LivingroomId].IsTurnedOn);
        }

        [TestMethod]
        public void MoveInRoomShouldNotTurnOnLightOnNightWhenWorkinghoursAreDaylight()
        {
            var (service, eventAggregator, scheduler, lampDictionary, dateTime) = SetupEnviroment(new AreaDescriptor { WorkingTime = WorkingTime.DayLight });
            var motionEvents = scheduler.CreateColdObservable
            (
              OnNext(Time.Tics(500), new MotionEnvelope(ToiletId)),
              OnNext(Time.Tics(1500), new MotionEnvelope(KitchenId)),
              OnNext(Time.Tics(2000), new MotionEnvelope(LivingroomId))
            );
            Mock.Get(eventAggregator).Setup(x => x.Observe<MotionEvent>()).Returns(motionEvents);
            Mock.Get(dateTime).Setup(x => x.Time).Returns(TimeSpan.FromHours(21));
            
            service.Initialize();

            var result = scheduler.Start(service.AnalyzeMove, 0, 0, long.MaxValue);

            Assert.AreEqual(false, lampDictionary[ToiletId].IsTurnedOn);
            Assert.AreEqual(false, lampDictionary[KitchenId].IsTurnedOn);
            Assert.AreEqual(false, lampDictionary[LivingroomId].IsTurnedOn);
        }

        [TestMethod]
        public void MoveInRoomShouldNotTurnOnLightOnDaylightWhenWorkinghoursIsNight()
        {
            var (service, eventAggregator, scheduler, lampDictionary, dateTime) = SetupEnviroment(new AreaDescriptor { WorkingTime = WorkingTime.AfterDusk });
            var motionEvents = scheduler.CreateColdObservable
            (
              OnNext(Time.Tics(500), new MotionEnvelope(ToiletId)),
              OnNext(Time.Tics(1500), new MotionEnvelope(KitchenId)),
              OnNext(Time.Tics(2000), new MotionEnvelope(LivingroomId))
            );
            Mock.Get(eventAggregator).Setup(x => x.Observe<MotionEvent>()).Returns(motionEvents);
            Mock.Get(dateTime).Setup(x => x.Time).Returns(TimeSpan.FromHours(12));

            service.Initialize();

            var result = scheduler.Start(service.AnalyzeMove, 0, 0, long.MaxValue);

            Assert.AreEqual(false, lampDictionary[ToiletId].IsTurnedOn);
            Assert.AreEqual(false, lampDictionary[KitchenId].IsTurnedOn);
            Assert.AreEqual(false, lampDictionary[LivingroomId].IsTurnedOn);
        }

        [TestMethod]
        public void MoveInRoomShouldNotTurnOnLightWhenAutomationIsDisabled()
        {
            var (service, eventAggregator, scheduler, lampDictionary, dateTime) = SetupEnviroment();
            var motionEvents = scheduler.CreateColdObservable
            (
              OnNext(Time.Tics(500), new MotionEnvelope(ToiletId))
            );
            Mock.Get(eventAggregator).Setup(x => x.Observe<MotionEvent>()).Returns(motionEvents);
            
            service.Initialize();
            service.DisableAutomation(ToiletId);

            var result = scheduler.Start(service.AnalyzeMove, 0, 0, long.MaxValue);

            Assert.AreEqual(false, lampDictionary[ToiletId].IsTurnedOn);
        }

        [TestMethod]
        public void MoveInRoomShouldTurnOnLightWhenAutomationIsReEnabled()
        {
            var (service, eventAggregator, scheduler, lampDictionary, dateTime) = SetupEnviroment();
            var motionEvents = scheduler.CreateColdObservable
            (
              OnNext(Time.Tics(500), new MotionEnvelope(ToiletId)),
              OnNext(Time.Tics(1500), new MotionEnvelope(ToiletId))
            );
            Mock.Get(eventAggregator).Setup(x => x.Observe<MotionEvent>()).Returns(motionEvents);

            service.Initialize();
            service.DisableAutomation(ToiletId);
            motionEvents.Subscribe(x => service.EnableAutomation(ToiletId));

            var result = scheduler.Start(service.AnalyzeMove, 0, 0, long.MaxValue);

            Assert.AreEqual(true, lampDictionary[ToiletId].IsTurnedOn);
        }


        [TestMethod]
        public void WhenLeaveFromOnePersonRoomWithNoConfusionShouldTurnOffLightImmediately()
        {
            var (service, eventAggregator, scheduler, lampDictionary, dateTime) = SetupEnviroment();
            var motionEvents = scheduler.CreateColdObservable
            (
              OnNext(Time.Tics(500), new MotionEnvelope(ToiletId)),
              OnNext(Time.Tics(1500), new MotionEnvelope(HallwayToiletId)),
              OnNext(Time.Tics(2000), new MotionEnvelope(HallwayLivingroomId))
            );
            Mock.Get(eventAggregator).Setup(x => x.Observe<MotionEvent>()).Returns(motionEvents);

            service.Initialize();

            var result = scheduler.Start(service.AnalyzeMove, 0, 0, long.MaxValue);

            Assert.AreEqual(false, lampDictionary[ToiletId].IsTurnedOn);

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
            LightAutomationService,
            IEventAggregator,
            TestScheduler,
            Dictionary<string, MotionLamp>,
            IDateTimeService
        )
        SetupEnviroment(AreaDescriptor areaDescription = null)
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

            var hallwayLampToilet = new MotionLamp();
            var hallwayLampLivingRoom = new MotionLamp();
            var toiletLamp = new MotionLamp();
            var livingRoomLamp = new MotionLamp();
            var bathroomLamp = new MotionLamp();
            var badroomLamp = new MotionLamp();
            var kitchenLamp = new MotionLamp();
            var balconyLamp = new MotionLamp();
            var staircaseLamp = new MotionLamp();

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
            var eventAggregator = Mock.Of<IEventAggregator>();
            var dateTimeService = Mock.Of<IDateTimeService>();
            var scheduler = new TestScheduler();
            var concurrencyProvider = new TestConcurrencyProvider(scheduler);
            var motionConfigurationProvider = new MotionConfigurationProvider();

            var lightAutomation = new LightAutomationService(eventAggregator, daylightService, logService, concurrencyProvider, dateTimeService,  motionConfigurationProvider);

            
            lightAutomation.RegisterDescriptor(hallwayDetectorToilet.Id, new[] { hallwayDetectorLivingRoom.Id, kitchenDetector.Id, staircaseDetector.Id }, hallwayLampToilet, area);
            lightAutomation.RegisterDescriptor(hallwayDetectorLivingRoom.Id, new[] { livingRoomDetector.Id, bathroomDetector.Id, hallwayDetectorToilet.Id }, hallwayLampLivingRoom, area);
            lightAutomation.RegisterDescriptor(livingRoomDetector.Id, new[] { balconyDetector.Id }, livingRoomLamp, area);
            lightAutomation.RegisterDescriptor(balconyDetector.Id, new[] { livingRoomDetector.Id }, balconyLamp, area);
            lightAutomation.RegisterDescriptor(kitchenDetector.Id, new[] { hallwayDetectorToilet.Id }, kitchenLamp, area);
            lightAutomation.RegisterDescriptor(bathroomDetector.Id, new[] { hallwayDetectorLivingRoom.Id }, bathroomLamp, area);
            lightAutomation.RegisterDescriptor(badroomDetector.Id, new[] { hallwayDetectorLivingRoom.Id }, badroomLamp, area);
            lightAutomation.RegisterDescriptor(staircaseDetector.Id, new[] { hallwayDetectorToilet.Id }, staircaseLamp, area);

            area.MaxPersonCapacity = 1;
            lightAutomation.RegisterDescriptor(toiletDetector.Id, new[] { hallwayDetectorToilet.Id }, toiletLamp, area);

            return
            (
                lightAutomation,
                eventAggregator,
                scheduler,
                lampDictionary,
                dateTimeService
            );
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

        public class MotionLamp : IComponent
        {
            
            public string Id => throw new NotImplementedException();

            public event EventHandler<ComponentFeatureStateChangedEventArgs> StateChanged;

            public bool IsTurnedOn { get; private set; }

            public void ExecuteCommand(ICommand command)
            {
                if (command is TurnOnCommand)
                {
                    IsTurnedOn = true;
                }
                else if (command is TurnOffCommand)
                {
                    IsTurnedOn = false;
                }
                else
                {
                    throw new NotSupportedException($"Not supported command {command}");
                }
            }

            public IComponentFeatureCollection GetFeatures()
            {
                throw new NotImplementedException();
            }

            public IComponentFeatureStateCollection GetState()
            {
                throw new NotImplementedException();
            }
        }
        #endregion
    }


}
