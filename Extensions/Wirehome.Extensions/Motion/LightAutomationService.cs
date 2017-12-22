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
using Wirehome.Contracts.Components;
using System.Collections.Immutable;

namespace Wirehome.Extensions
{
    public class LightAutomationService : IService, IDisposable
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IDaylightService _daylightService;
        private readonly ILogger _logger;
        private readonly ImmutableLazyDictionary<string, MotionDescriptor> _motionDescriptors = new ImmutableLazyDictionary<string, MotionDescriptor>();
        private readonly DisposeContainer _disposeContainer = new DisposeContainer();
        private readonly IConcurrencyProvider _concurrencyProvider;
        private readonly IMotionConfigurationProvider _motionConfigurationProvider;
        private readonly IDateTimeService _dateTimeService;

        public int NumberOfPersonsInHouse { get; }
        private bool _IsInitialized;

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
            _motionDescriptors.Initialize();

            _motionDescriptors.ForEach(room => room.BuildNeighborsCache(GetNeighbors(room.MotionDetectorUid)));

            if (_motionDescriptors.Count == 0) throw new Exception("No detectors found to automate");

            var missingDescriptions = _motionDescriptors.Select(m => m.Value)
                                                        .SelectMany(n => n.Neighbors)
                                                        .Distinct()
                                                        .Except(_motionDescriptors.Keys)
                                                        .ToList();

            if(missingDescriptions.Count > 0) throw new Exception($"Following neighbors have not registred descriptors: {string.Join(", ", missingDescriptions)}");

            _IsInitialized = true;
            return Task.CompletedTask;
        }

        public MotionDescriptor RegisterDescriptor(string motionDetectorUid, IEnumerable<string> neighbors, IComponent lamp, AreaDescriptor initializer = null)
        {
            if (_IsInitialized) throw new Exception("Cannot register new descriptors after service has started");

            //TODO wait for new component implementation
            //if (lamp?.GetFeatures()?.Supports<PowerStateFeature>() ?? false) throw new Exception($"Component {lamp?.Id} is not supporting PowerStateFeature");
            var descriptor = new MotionDescriptor(motionDetectorUid, neighbors, lamp, _concurrencyProvider.Scheduler, _daylightService, _dateTimeService, initializer);

            _motionDescriptors.Add(descriptor.MotionDetectorUid, descriptor);
            return descriptor;
        }

        public IObservable<MotionVector> AnalyzeMove()
        {
           var events = _eventAggregator.Observe<MotionEvent>();

            return events.Timestamp(_concurrencyProvider.Scheduler)
                         .Select(move => new MotionPoint(move.Value.Message.MotionDetectorUID, move.Timestamp))
                         .Do(point => _motionDescriptors?[point.Uid]?.MarkMotion(point.TimeStamp))
                         .Buffer(events, x => Observable.Timer(TimeSpan.FromMilliseconds(_motionConfigurationProvider.MotionTimeWindow), _concurrencyProvider.Scheduler))
                         .Where(buffer => buffer.Count > 1)
                         .Select(movments => Enumerable.Repeat(movments.Skip(1).FirstOrDefault(move => IsEndPoint(movments[0], move))?.ToVector(movments[0]), 1)
                                                      ?.Where(vector => vector != null)
                                                      ?.Select(vector => vector.RegisterMotionConfusions(GetMovementsInNeighborhood(vector)))
                                                      ?.FirstOrDefault())
                         .Do(RegisterVector);
        }

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

        public void DisableAutomation(string roomId) => _motionDescriptors?[roomId].DisableAutomation();
        public void EnableAutomation(string roomId) => _motionDescriptors?[roomId].EnableAutomation();
        
        private bool IsEndPoint(MotionPoint start, MotionPoint potencialEnd) => AreNeighbors(start, potencialEnd) && IsMovePhisicallyPosible(start, potencialEnd);
        private bool AreNeighbors(MotionPoint p1, MotionPoint p2) => _motionDescriptors?[p1.Uid]?.Neighbors?.Contains(p2.Uid) ?? false;
        private bool IsMovePhisicallyPosible(MotionPoint p1, MotionPoint p2) => (p2.TimeStamp - p1.TimeStamp).TotalMilliseconds >= _motionConfigurationProvider.MotionMinDiff;
        private IEnumerable<MotionDescriptor> GetNeighbors(string roomId) => _motionDescriptors.Where(x => _motionDescriptors[roomId].Neighbors.Contains(x.Key)).Select(y => y.Value);

        private void RegisterVector(MotionVector vector)
        {
            _motionDescriptors[vector.Start.Uid].MarkLeave(vector);
            _motionDescriptors[vector.End.Uid].MarkEnter(vector);
        }

        public void Dispose() => _disposeContainer.Dispose();

    }
}