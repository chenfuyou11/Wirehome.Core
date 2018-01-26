using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Linq;
using System.Collections.Immutable;
using Wirehome.Contracts.Services;
using Wirehome.Contracts.Logging;
using Wirehome.Contracts.Environment;
using Wirehome.Extensions.Core;
using Wirehome.Extensions.Messaging.Core;
using Wirehome.Contracts.Core;
using Wirehome.Extensions.Extensions;
using Force.DeepCloner;
using Wirehome.Motion.Model;

namespace Wirehome.Motion
{
    public class LightAutomationService : IService, IDisposable
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IDaylightService _daylightService;
        private readonly ILogger _logger;
        private readonly IConcurrencyProvider _concurrencyProvider;
        private readonly IDateTimeService _dateTimeService;
        private readonly IObservableTimer _observableTimer;
        private MotionConfiguration _motionConfiguration;
        private ImmutableDictionary<string, Room> _rooms;
        private readonly List<ConfusedVector> _confusedVectors = new List<ConfusedVector>();
        private readonly DisposeContainer _disposeContainer = new DisposeContainer();
        private readonly TaskCompletionSource<bool> _workDoneTaskSource = new TaskCompletionSource<bool>();

        private bool _IsInitialized;
        public Task StopWorking => _workDoneTaskSource.Task;


        public LightAutomationService(IEventAggregator eventAggregator,
                                      IDaylightService daylightService,
                                      ILogService logService,
                                      IConcurrencyProvider concurrencyProvider,
                                      IDateTimeService dateTimeService,
                                      IMotionConfigurationProvider motionConfigurationProvider,
                                      IObservableTimer observableTimer
        )
        {
            if (logService == null) throw new ArgumentNullException(nameof(logService));
            _logger = logService.CreatePublisher(nameof(LightAutomationService));
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            _daylightService = daylightService ?? throw new ArgumentNullException(nameof(daylightService));
            _concurrencyProvider = concurrencyProvider ?? throw new ArgumentNullException(nameof(concurrencyProvider));
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));

            var configurationProvider = motionConfigurationProvider ?? throw new ArgumentNullException(nameof(motionConfigurationProvider));
            _motionConfiguration = configurationProvider.GetConfiguration();
            _observableTimer = observableTimer;
        }

        public Task Initialize()
        {
            _IsInitialized = true;
            return Task.CompletedTask;
        }

        public void RegisterRoom(IEnumerable<RoomInitializer> roomInitializers)
        {
            if (_IsInitialized) throw new Exception("Cannot register new descriptors after service has started");

            if (!roomInitializers.Any()) throw new Exception("No detectors found to automate");

            //TODO Check if component is real lamp - wait for new component implementation
            _rooms = roomInitializers.Select(roomInitializer => roomInitializer.ToRoom(_motionConfiguration, _concurrencyProvider.Scheduler, _daylightService, _dateTimeService))
                                                             .ToImmutableDictionary(k => k.Uid, v => v);

            var missingRooms = _rooms.Select(m => m.Value)
                                                        .SelectMany(n => n.Neighbors)
                                                        .Distinct()
                                                        .Except(_rooms.Keys)
                                                        .ToList();
            if (missingRooms.Count > 0) throw new Exception($"Following neighbors have not registered rooms: {string.Join(", ", missingRooms)}");

            _rooms.Values.ForEach(md => md.BuildNeighborsCache(GetNeighbors(md.Uid)));
        }

        public void DisableAutomation(string roomId) => _rooms?[roomId].DisableAutomation();
        public void DisableAutomation(string roomId, TimeSpan time) => _rooms?[roomId].DisableAutomation(time);
        public void EnableAutomation(string roomId) => _rooms?[roomId].EnableAutomation();
        public void Dispose() => _disposeContainer.Dispose();
        public int GetCurrentNumberOfPeople(string roomId) => _rooms[roomId].NumberOfPersonsInArea;
        public int NumberOfPersonsInHouse => _rooms.Sum(md => md.Value.NumberOfPersonsInArea);
        public AreaDescriptor GetAreaDescriptor(string roomId) => _rooms[roomId].AreaDescriptor.ShallowClone();
        public int NumberOfConfusions => _confusedVectors.Count;

        public void Start()
        {
            _disposeContainer.Add(PeriodicCheck());
            _disposeContainer.Add(CheckMotion());
        }

        public IObservable<MotionVector> AnalyzeMove()
        {
            var events = _eventAggregator.Observe<MotionEvent>();

            return events.Timestamp(_concurrencyProvider.Scheduler)
                         .Select(move => new MotionWindow(move.Value.Message.MotionDetectorUID, move.Timestamp))
                         .Do(HandleMove)
                         .Window(events, _ => Observable.Timer(_motionConfiguration.MotionTimeWindow, _concurrencyProvider.Scheduler))
                         .SelectMany(x => x.Scan((vectors, currentPoint) => vectors.AccumulateVector(currentPoint.Start, IsProperVector))
                         .SelectMany(motion => motion.ToVectors()));
        }

        private void HandleMove(MotionWindow point)
        {
            ResolveConfusion(point.Start);
            _rooms?[point.Start.Uid]?.MarkMotion(point.Start.TimeStamp);
        }

        private void ResolveConfusion(MotionPoint point)
        {
            for (int i = _confusedVectors.Count - 1; i >= 0; i--)
            {
                var confusedVector = _confusedVectors[i];
                // If we have move in start point we are sure that leave was not there (TODO what about more than 2 people)
                if (confusedVector.Vector.Start == point)
                {
                    _confusedVectors.Remove(confusedVector);
                }
                // If we can eliminate confusion we try to reduce it and check if it is still confusing
                else if (confusedVector.ConfusionPoint.Contains(point) && !confusedVector.ReduceConfusion(point).IsConfused)
                {
                    _confusedVectors.Remove(confusedVector);
                    MarkVector(confusedVector.Vector);
                }
            }
        }

        private IDisposable PeriodicCheck() => _observableTimer.GenerateTime(_motionConfiguration.PeriodicCheckTime)
                                                               .ObserveOn(_concurrencyProvider.Task)
                                                               .Subscribe(_ => _rooms.Values.ForEach(md => md.Update()), HandleError);


        private IDisposable CheckMotion() => AnalyzeMove().ObserveOn(_concurrencyProvider.Task).Subscribe(HandleVector, HandleError, () => _workDoneTaskSource.SetResult(true));

        private void HandleError(Exception ex) => _logger.Error(ex, "Exception in LightAutomationService");


        private void HandleVector(MotionVector motionVector)
        {
            var confusionPoints = _rooms[motionVector.End.Uid].GetMovementsInNeighborhood(motionVector);

            if (confusionPoints.Count == 0)
            {
                _logger.Info(motionVector.ToString());
                MarkVector(motionVector);
            }
            // If there is no more people in are this confusion is resolving immediately
            else if (_rooms[motionVector.Start.Uid].NumberOfPersonsInArea > 0)
            {
                _logger.Info($"{motionVector} [Confused]");
                _confusedVectors.Add(new ConfusedVector(motionVector, confusionPoints));
            }
            else
            {
                _logger.Info($"{motionVector} [Deleted]");
            }
        }

        private void MarkVector(MotionVector motionVector)
        {
            // If there was a vector from this room we don't start another
            if (_rooms[motionVector.End.Uid].LastVectorEnter?.EqualsWithStartTime(motionVector) ?? false) return;

            _rooms[motionVector.Start.Uid].MarkLeave(motionVector);
            _rooms[motionVector.End.Uid].MarkEnter(motionVector);
        }

        private bool IsProperVector(MotionPoint start, MotionPoint potencialEnd) => AreNeighbors(start, potencialEnd) && IsMovePhisicallyPosible(start, potencialEnd);
        private bool AreNeighbors(MotionPoint p1, MotionPoint p2) => _rooms?[p1.Uid]?.Neighbors?.Contains(p2.Uid) ?? false;
        private bool IsMovePhisicallyPosible(MotionPoint p1, MotionPoint p2) => p2.TimeStamp - p1.TimeStamp >= _motionConfiguration.MotionMinDiff;
        private IEnumerable<Room> GetNeighbors(string roomId) => _rooms.Where(x => _rooms[roomId].Neighbors.Contains(x.Key)).Select(y => y.Value);

    }
}