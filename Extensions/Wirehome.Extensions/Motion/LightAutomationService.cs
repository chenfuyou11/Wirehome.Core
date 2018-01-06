using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Linq;
using System.Threading;
using System.Collections.Immutable;
using Wirehome.Contracts.Services;
using Wirehome.Extensions.MotionModel;
using Wirehome.Contracts.Logging;
using Wirehome.Contracts.Environment;
using Wirehome.Extensions.Core;
using Wirehome.Extensions.Messaging.Core;
using Wirehome.Contracts.Core;
using Wirehome.Extensions.Extensions;
using System.Reactive.Concurrency;

namespace Wirehome.Extensions
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
        private ImmutableDictionary<string, MotionDescriptor> _motionDescriptors;
        private readonly List<ConfusedVector> _confusedVectors = new List<ConfusedVector>();
        private readonly DisposeContainer _disposeContainer = new DisposeContainer();
        private readonly TaskCompletionSource<bool> _workDoneTaskSource = new TaskCompletionSource<bool>();
        private bool _IsInitialized;
        public Task StopWorking => _workDoneTaskSource.Task;
        public int NumberOfPersonsInHouse { get; }

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

        public void RegisterDescriptors(IEnumerable<MotionDesctiptorInitializer> motionDescriptorsInitilizers)
        {
            if (_IsInitialized) throw new Exception("Cannot register new descriptors after service has started");
            if (!motionDescriptorsInitilizers.Any()) throw new Exception("No detectors found to automate");

            //TODO Check if component is real lamp - wait for new component implementation
            _motionDescriptors = motionDescriptorsInitilizers.Select(md => md.ToMotionDescriptor(_motionConfiguration, _concurrencyProvider.Scheduler, _daylightService, _dateTimeService))
                                                             .ToImmutableDictionary(k => k.MotionDetectorUid, v => v);
   
            var missingDescriptions = _motionDescriptors.Select(m => m.Value)
                                                        .SelectMany(n => n.Neighbors)
                                                        .Distinct()
                                                        .Except(_motionDescriptors.Keys)
                                                        .ToList();
            if (missingDescriptions.Count > 0) throw new Exception($"Following neighbors have not registered descriptors: {string.Join(", ", missingDescriptions)}");

            _motionDescriptors.Values.ForEach(md => md.BuildNeighborsCache(GetNeighbors(md.MotionDetectorUid)));
        }

        public void DisableAutomation(string roomId) => _motionDescriptors?[roomId].DisableAutomation();
        public void DisableAutomation(string roomId, TimeSpan time) => _motionDescriptors?[roomId].DisableAutomation(time);
        public void EnableAutomation(string roomId) => _motionDescriptors?[roomId].EnableAutomation();
        public void Dispose() => _disposeContainer.Dispose();
        public int GetCurrentNumberOfPeople(string roomId) => _motionDescriptors[roomId].NumberOfPersonsInArea;

        public void Start()
        {
            _disposeContainer.Add(CheckInactivity());
            _disposeContainer.Add(CheckMotion());
        }

        public IObservable<MotionVector> AnalyzeMove()
        {
            var events = _eventAggregator.Observe<MotionEvent>();

            return events.Timestamp(_concurrencyProvider.Scheduler)
                        .Select(move => new Motion(new MotionPoint(move.Value.Message.MotionDetectorUID, move.Timestamp)))
                        .Do(point =>
                        {
                            ResolveConfusion(point.Start);
                            _motionDescriptors?[point.Start.Uid]?.MarkMotion(point.Start.TimeStamp);
                        })
                        .Window(events, timeWindow => Observable.Timer(_motionConfiguration.MotionTimeWindow, _concurrencyProvider.Scheduler))
                        .SelectMany(x => x.Scan((accumulate, actualPoint) =>
                        {
                            if (accumulate == null || accumulate.Vector != null) return null;
                            // Fix - works only in one neighbor move - if we have move in two neighbors it will not work
                            if (IsEndPoint(accumulate.Start, actualPoint.Start))
                            {
                                accumulate.CreateVector(actualPoint.Start);
                            }
                            return accumulate;
                        })
                        .Where(motion => motion != null && motion.Vector != null))
                        .Select(motion => motion.Vector);
        }
        
        private void ResolveConfusion(MotionPoint point)
        {
            for (int i = _confusedVectors.Count - 1; i >= 0; i--)
            {
                var confusedVector = _confusedVectors[i];
                // If we have move in start point we are sure that leave was not there (TODO what about more than 2 people)
                if(confusedVector.Vector.Start == point)
                {
                    _confusedVectors.Remove(confusedVector);
                }
                // If we can eliminate confusion we try to reduce it and check if it is still confusing
                else if(confusedVector.ConfusionPoint.Contains(point) && !confusedVector.ReduceConfusion(point).IsConfused)
                {
                    _confusedVectors.Remove(confusedVector);
                    MarkVector(confusedVector.Vector);
                }
            }
        }
        
        private IDisposable CheckInactivity()
        {
            return _observableTimer.GenerateTime(_motionConfiguration.PeriodicCheckTime)
                                   .ObserveOn(_concurrencyProvider.Task)
                                   .Subscribe(_ => _motionDescriptors.Values.ForEach(md => md.Update()), HandleError, Test);
        }

        private IDisposable CheckMotion()
        {
            return AnalyzeMove().ObserveOn(_concurrencyProvider.Task)
                                .Subscribe(HandleVector, HandleError, () => _workDoneTaskSource.SetResult(true));
        }

        private void HandleError(Exception ex)
        {
            _logger.Error(ex, "Exception in LightAutomationService");
        }

        private void Test()
        {

        }

        private void HandleVector(MotionVector motionVector)
        {
            var confusionPoints = GetMovementsInNeighborhood(motionVector);

            if (!confusionPoints.Any())
            {
                MarkVector(motionVector);
            }
            else
            {
                _confusedVectors.Add(new ConfusedVector(motionVector, confusionPoints));
            }
        }

        private void MarkVector(MotionVector motionVector)
        {
            _motionDescriptors[motionVector.Start.Uid].MarkLeave(motionVector);
            _motionDescriptors[motionVector.End.Uid].MarkEnter(motionVector);
        }
        
        private IEnumerable<MotionPoint> GetMovementsInNeighborhood(MotionVector vector)
        {
            return _motionDescriptors[vector.End.Uid].NeighborsCache
                                                     .Where(n => n.MotionDetectorUid != vector.Start.Uid && vector.End.TimeStamp - n.LastMotionTime < n.MotionDetectorAlarmTime)
                                                     .Select(n => new MotionPoint(n.MotionDetectorUid, n.LastMotionTime.GetValueOrDefault()));
        }

        private bool IsEndPoint(MotionPoint start, MotionPoint potencialEnd) => AreNeighbors(start, potencialEnd) && IsMovePhisicallyPosible(start, potencialEnd);
        private bool AreNeighbors(MotionPoint p1, MotionPoint p2) => _motionDescriptors?[p1.Uid]?.Neighbors?.Contains(p2.Uid) ?? false;
        private bool IsMovePhisicallyPosible(MotionPoint p1, MotionPoint p2) => p2.TimeStamp - p1.TimeStamp >= _motionConfiguration.MotionMinDiff;
        private IEnumerable<MotionDescriptor> GetNeighbors(string roomId) => _motionDescriptors.Where(x => _motionDescriptors[roomId].Neighbors.Contains(x.Key)).Select(y => y.Value);

        //private IEnumerable<MotionPoint> GetMovementsInNeighborhoodOld(MotionVector vector)
        //{
        //    return _motionDescriptors[vector.End.Uid].Neighbors
        //                                             .Where(n => n != vector.Start.Uid)
        //                                             .SelectMany(neighbor => _motionDescriptors[neighbor].GetLastMovments(vector.End.TimeStamp));
        //}
    }
}