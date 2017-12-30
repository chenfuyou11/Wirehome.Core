using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Linq;
using Wirehome.Contracts.Services;
using Wirehome.Extensions.MotionModel;
using Wirehome.Contracts.Logging;
using Wirehome.Contracts.Environment;
using Wirehome.Extensions.Core;
using Wirehome.Extensions.Messaging.Core;
using Wirehome.Contracts.Core;
using System.Collections.Immutable;
using Wirehome.Extensions.Extensions;
using System.Threading;

namespace Wirehome.Extensions
{
    public class LightAutomationService : IService, IDisposable
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IDaylightService _daylightService;
        private readonly ILogger _logger;
        private readonly IConcurrencyProvider _concurrencyProvider;
        private readonly IMotionConfigurationProvider _motionConfigurationProvider;
        private readonly IDateTimeService _dateTimeService;
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
                                      IMotionConfigurationProvider motionConfigurationProvider
        )
        {
            if (logService == null) throw new ArgumentNullException(nameof(logService));
            _logger = logService.CreatePublisher(nameof(LightAutomationService));
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            _motionConfigurationProvider = motionConfigurationProvider ?? throw new ArgumentNullException(nameof(motionConfigurationProvider));
            _daylightService = daylightService ?? throw new ArgumentNullException(nameof(daylightService));
            _concurrencyProvider = concurrencyProvider ?? throw new ArgumentNullException(nameof(concurrencyProvider));
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
            
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

            //TODO Check if component is real lamo - wait for new component implementation
            _motionDescriptors = motionDescriptorsInitilizers.Select(md => md.ToMotionDescriptor(_concurrencyProvider.Scheduler, _daylightService, _dateTimeService))
                                                             .ToImmutableDictionary(k => k.MotionDetectorUid, v => v);

            foreach (var md in _motionDescriptors.Values)
            {
                md.BuildNeighborsCache(GetNeighbors(md.MotionDetectorUid));
            }
            
            var missingDescriptions = _motionDescriptors.Select(m => m.Value)
                                                        .SelectMany(n => n.Neighbors)
                                                        .Distinct()
                                                        .Except(_motionDescriptors.Keys)
                                                        .ToList();
            if (missingDescriptions.Count > 0) throw new Exception($"Following neighbors have not registred descriptors: {string.Join(", ", missingDescriptions)}");
        }

        public IObservable<MotionVector> AnalyzeMoveOptimized()
        {
            var events = _eventAggregator.Observe<MotionEvent>();

            return events.Timestamp(_concurrencyProvider.Scheduler)
                        .Select(move => new Motion(new MotionPoint(move.Value.Message.MotionDetectorUID, move.Timestamp)))
                        .Do(point => _motionDescriptors?[point.Start.Uid]?.MarkMotion(point.Start.TimeStamp))
                        .Window(events, timeWindow => Observable.Timer(_motionConfigurationProvider.MotionTimeWindow, _concurrencyProvider.Scheduler))
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



        public IObservable<MotionVector> AnalyzeMove()
        {
            var events = _eventAggregator.Observe<MotionEvent>();

            Console.WriteLine($"Thread: {Thread.CurrentThread.ManagedThreadId} Start");

            return events.Timestamp(_concurrencyProvider.Scheduler)
                         .SubscribeOn(_concurrencyProvider.Task)
                         .Select(move => new MotionPoint(move.Value.Message.MotionDetectorUID, move.Timestamp))
                         .Do(point => 
                         {
                             Console.WriteLine($"Thread: {Thread.CurrentThread.ManagedThreadId} Do point: {point}");
                             ResolveConfusion(point);
                             _motionDescriptors?[point.Uid]?.MarkMotion(point.TimeStamp);
                          })
                         .Buffer(events, x => Observable.Timer(_motionConfigurationProvider.MotionTimeWindow, _concurrencyProvider.Scheduler))
                         .Where(buffer => buffer.Count > 1)
                         .Select(movments => Enumerable.Repeat(movments.Skip(1).FirstOrDefault(move => IsEndPoint(movments[0], move))?.ToVector(movments[0]), 1)
                                                    ?.Where(vector => vector != null)) // Chcek for move from one room to two neighbors at same time
                         .SelectMany(vector => vector)
                         .Where(vector => vector != null);
        }

        private void ResolveConfusion(MotionPoint point)
        {
            _confusedVectors.RemoveAll(cv => cv.Vector.Start == point);

            for (int i = _confusedVectors.Count; i >= 0; i--)
            {
                var canceledVector = _confusedVectors[i];
                if(canceledVector.ConfusionPoint.Contains(point))
                {
                    canceledVector.ReduceConfusion(point);
                   
                }
            }
        }

        public void Start()
        {
            _disposeContainer.Add(CheckInactivity());
            _disposeContainer.Add(CheckMotion());
        }

        private IDisposable CheckInactivity()
        {
            return Observable.Timer(_motionConfigurationProvider.PeriodicCheckTime, _concurrencyProvider.Scheduler)
                             .Timestamp()
                             .Select(time => time.Timestamp)
                             .ObserveOn(_concurrencyProvider.Task)
                             .Subscribe(time => _motionDescriptors.Values.ForEach(md => md.ResolveCofusion(time)), HandleError);
        }

        private IDisposable CheckMotion()
        {
            return AnalyzeMove().ObserveOn(_concurrencyProvider.Task)
                                .Subscribe(HandleVector, HandleError, HandleComplete);
        }

        private void HandleError(Exception ex)
        {
            _logger.Error(ex, "Exception in LightAutomationService");
        }

        private void HandleComplete()
        {
            _workDoneTaskSource.SetResult(true);
        }

        private void HandleVector(MotionVector motionVector)
        {
            Console.WriteLine($"Thread: {Thread.CurrentThread.ManagedThreadId} Vector: {motionVector}");
            var confusionPoints = GetMovementsInNeighborhood(motionVector);
            if(!confusionPoints.Any())
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

        public void DisableAutomation(string roomId) => _motionDescriptors?[roomId].DisableAutomation();
        public void EnableAutomation(string roomId) => _motionDescriptors?[roomId].EnableAutomation();
        public void Dispose() => _disposeContainer.Dispose();
        public int GetCurrentNumberOfPeople(string roomId) => _motionDescriptors[roomId].NumberOfPersonsInArea;


        /// <summary>
        /// Search in all neighbors of END diffrent of START for any movemnts in last period time
        /// </summary>
        /// <param name="vector">Vector we are cheking out</param>
        /// <returns>List of all motionpoint in neighborhoo</returns>
        private IEnumerable<MotionPoint> GetMovementsInNeighborhood(MotionVector vector)
        {
            return _motionDescriptors[vector.End.Uid].Neighbors
                                                     .Where(n => n != vector.Start.Uid)
                                                     .SelectMany(neighbor => _motionDescriptors[neighbor].GetLastMovments(vector.End.TimeStamp));
        }

        private IEnumerable<MotionPoint> GetMovementsInNeighborhood2(MotionVector vector)
        {
            return _motionDescriptors[vector.End.Uid].NeighborsCache
                                                     .Where(n => n.MotionDetectorUid != vector.Start.Uid && vector.End.TimeStamp - n.LastMotionTime < n.MotionDetectorAlarmTime)
                                                     .Select(n => new MotionPoint(n.MotionDetectorUid, n.LastMotionTime));
        }

        private bool IsEndPoint(MotionPoint start, MotionPoint potencialEnd) => AreNeighbors(start, potencialEnd) && IsMovePhisicallyPosible(start, potencialEnd);
        private bool AreNeighbors(MotionPoint p1, MotionPoint p2) => _motionDescriptors?[p1.Uid]?.Neighbors?.Contains(p2.Uid) ?? false;
        private bool IsMovePhisicallyPosible(MotionPoint p1, MotionPoint p2) => p2.TimeStamp - p1.TimeStamp >= _motionConfigurationProvider.MotionMinDiff;
        private IEnumerable<MotionDescriptor> GetNeighbors(string roomId) => _motionDescriptors.Where(x => _motionDescriptors[roomId].Neighbors.Contains(x.Key)).Select(y => y.Value);

        
    }

    public class Motion
    {
        public Motion(MotionPoint start)
        {
            Start = start;
        }

        public MotionPoint Start { get; }
        public MotionVector Vector { get; private set; }

        public void CreateVector(MotionPoint mp)
        {
            Vector = new MotionVector(Start, mp);
        }

        public override string ToString()
        {
            return $"{Start} || {Vector?.ToString() ?? "<>"}";
        }
    }

    public class ConfusedVector
    {
        private readonly List<MotionPoint> _confusionPoints;

        public ConfusedVector(MotionVector vector, IEnumerable<MotionPoint> confusionPoints)
        {
            Vector = vector;
            _confusionPoints = new List<MotionPoint>(confusionPoints);
        }

        public ConfusedVector ReduceConfusion(MotionPoint confusionPoint)
        {
            _confusionPoints.Remove(confusionPoint);
            return this;
        }

        public bool IsConfused => _confusionPoints.Count > 0;

        public MotionVector Vector { get; }
        public IReadOnlyCollection<MotionPoint> ConfusionPoint => _confusionPoints.AsReadOnly();
    }
}