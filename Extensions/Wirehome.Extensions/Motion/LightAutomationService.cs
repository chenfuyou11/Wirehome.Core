using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Diagnostics;
using System.Linq;
using Wirehome.Contracts.Services;
using Wirehome.Extensions.MotionModel;
using Wirehome.Contracts.Components.Features;
using Wirehome.Contracts.Logging;
using Wirehome.Contracts.Environment;
using Wirehome.Contracts.Scheduling;
using Wirehome.Extensions.Core;
using Wirehome.Extensions.Messaging.Core;
using Wirehome.Contracts.Actuators;
using Wirehome.Contracts.Core;

namespace Wirehome.Extensions
{
    public class LightAutomationService : IService, IDisposable
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly ISchedulerService _schedulerService;
        private readonly IDaylightService _daylightService;
        private readonly ILogger _logger;
        private readonly Dictionary<string, MotionDescriptor> _motionDescriptors = new Dictionary<string, MotionDescriptor>();
        private readonly DisposeContainer _disposeContainer = new DisposeContainer();
        private readonly IConcurrencyProvider _concurrencyProvider;
        private readonly IMotionConfigurationProvider _motionConfigurationProvider;
        private readonly IDateTimeService _dateTimeService;

        public int NumberOfPersonsInHouse { get; private set; }
        private bool _IsInitialized;
        private const string MOTION_TIMER = "MOTION_TIMER";
        
        public LightAutomationService(IEventAggregator eventAggregator,
                                      ISchedulerService schedulerService,
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
            _schedulerService = schedulerService ?? throw new ArgumentNullException(nameof(schedulerService));
            _daylightService = daylightService ?? throw new ArgumentNullException(nameof(daylightService));
            _concurrencyProvider = concurrencyProvider ?? throw new ArgumentNullException(nameof(concurrencyProvider));
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
        }

        public Task Initialize()
        {
            _schedulerService.Register(MOTION_TIMER, TimeSpan.FromSeconds(1), (Action)MotionScheduler);

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

        public MotionDescriptor RegisterDescriptor(string motionDetectorUid, IEnumerable<string> neighbors, ILamp lamp, AreaDescriptor initializer = null)
        {
            if (_IsInitialized) throw new Exception("Cannot register new descriptors after service has started");
            
            if (lamp?.GetFeatures()?.Supports<PowerStateFeature>() ?? false) throw new Exception($"Component {lamp?.Id} is not supporting PowerStateFeature");
            var descriptor = new MotionDescriptor(motionDetectorUid, neighbors, lamp, _concurrencyProvider.Scheduler, _daylightService, _dateTimeService, initializer);

            _motionDescriptors.Add(descriptor.MotionDetectorUid, descriptor);
            return descriptor;
        }

        public void MotionScheduler()
        {
            var ms = Stopwatch.StartNew();
            ms.Start();

            foreach(var descriptor in _motionDescriptors.Values)
            {
                
            }

            ms.Stop();
            _logger.Info($"MotionScheduler time {ms.ElapsedMilliseconds}ms");
        }
  
        public IObservable<MotionVector> AnalyzeMove()
        {
            var me = _eventAggregator.Observe<MotionEvent>();

            return me.Timestamp(_concurrencyProvider.Scheduler)
                     .Select(move => new MotionPoint(move.Value.Message.MotionDetectorUID, move.Timestamp))
                     .Do(point =>
                     {
                         _motionDescriptors?[point.Uid]?.MarkMotion(point.TimeStamp);
                     })
                     .Buffer(me, (x) => Observable.Timer(TimeSpan.FromMilliseconds(_motionConfigurationProvider.MotionTimeWindow), _concurrencyProvider.Scheduler))
                     .Select(movments =>
                     {
                        MotionVector vector = null;
                        
                        foreach (var move in movments)
                        {
                           if (vector == null)
                           {
                              vector = new MotionVector(move);
                              continue;
                           }
                           
                           if
                           (
                              AreNeighbors(vector.Start, move) &&
                              IsPhisicallyPosible(vector.Start, move) && 
                              !vector.IsComplete()
                           )
                           {
                              vector.SetEnd(move);
                              break;
                           }
                        }

                        if(vector.IsComplete())
                        {
                            // If there is move in neighborhood of END point other then START it means thet vector couldbe not real
                            // becouse move can be from other staring point
                            vector.RegisterMotionConfusions(GetMovementsInNeighborhood(vector));
                        }
                        
                        
                        return vector;
                     })
                     .Where(vector => vector != null && vector.IsComplete());
            //.Where(y => y.Path.Count > 1)
            //.DistinctUntilChanged();

            
            //var resource = motion.Subscribe(x =>
            //{
            //    var time = DateTime.Now;
            //    Console.WriteLine($"[{time.Minute}:{time.Second}:{time.Millisecond}] {x}");
            //    Console.WriteLine(x);

            //});

            //_disposeContainer.Add(resource);   
        }

        private bool AreNeighbors(MotionPoint p1, MotionPoint p2) => _motionDescriptors?[p1.Uid]?.Neighbors?.Contains(p2.Uid) ?? false;

        private bool IsPhisicallyPosible(MotionPoint p1, MotionPoint p2) => (p2.TimeStamp - p1.TimeStamp).TotalMilliseconds > _motionConfigurationProvider.MotionMinDiff;

        /// <summary>
        /// Search in all neighbors of END diffrent of START for any movemnts in last period time
        /// </summary>
        /// <param name="v">Vector we are cheking out</param>
        /// <returns>List of all motionpoint in neighborhoo</returns>
        private List<MotionPoint> GetMovementsInNeighborhood(MotionVector v)
        {
            var neighbors = new List<MotionPoint>();
            if (v.IsComplete())
            {
                foreach (var neighbor in _motionDescriptors[v.End.Uid].Neighbors.Where(n => n != v.Start.Uid))
                {
                    neighbors.AddRange(_motionDescriptors[neighbor]
                             .MotionHistory
                             .GetLastElements(TimeSpan.FromMilliseconds(_motionConfigurationProvider.MotionTimeWindow))
                             .Select(time => new MotionPoint(neighbor, time)));
                }
            }
            return neighbors;
        }

        public void Dispose()
        {
            _disposeContainer.Dispose();
        }
    }
}
