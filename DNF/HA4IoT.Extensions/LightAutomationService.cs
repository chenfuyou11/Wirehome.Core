using System;
using System.Collections.Generic;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Sensors;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Triggers;
using System.Reactive.Linq;
using System.Linq;
using HA4IoT.Extensions.MotionModel;
using HA4IoT.Extensions.Extensions;

namespace HA4IoT.Extensions
{
    public class LightAutomationService : IService, IDisposable
    {
        //Add observable with motion vectors detected
        //Add property with number of persons
        private const int MOTION_TIME_WINDOW = 3000;
        private const int MOTION_TIME_SHIFT = 200;

        private readonly Dictionary<IMotionDetector, MotionDescriptor> _motionDetectors = new Dictionary<IMotionDetector, MotionDescriptor>();
        private readonly IAreaService _areaService;
        private readonly List<IDisposable> _resources = new List<IDisposable>();

        public LightAutomationService(IAreaService areaService)
        {
            _areaService = areaService;
        }

        public void Startup()
        {
            FindRegistredMotionDetectors();
        }

        private void FindRegistredMotionDetectors()
        {
            foreach (var area in _areaService.GetAreas())
            {
                foreach (var motionDetector in area.GetComponents<IMotionDetector>())
                {
                    _motionDetectors[motionDetector] = new MotionDescriptor
                    {
                        Area = area
                    };
                }
            }
        }

        public MotionDescriptor ConfigureMotionDetector(IMotionDetector motionDetector, IMotionDetector neighbor, IActuator acutatot)
        {
             if(!_motionDetectors.ContainsKey(motionDetector))
             {
                throw new Exception("This motion detector was not register in any area");
             }

            var descriptor = _motionDetectors[motionDetector];
            descriptor.MotionDetector = motionDetector;
            descriptor.Neighbor = neighbor;
            descriptor.Acutator = acutatot;

            var trigger = motionDetector.GetMotionDetectedTrigger();

            descriptor.MotionSource = Observable.FromEventPattern<TriggeredEventArgs>
            (
                h => trigger.Triggered += h,
                h => trigger.Triggered -= h).Select(x => descriptor
            );

            return descriptor;
        }
        

        public void StartWatchForMove()
        {
            if (_motionDetectors.All(x => x.Value.MotionSource == null))
            {
                throw new Exception("First you have to add motion detectors with ConfigureMotionDetector");
            }

            var detectors = _motionDetectors.Values
                                            .Where(s => s.MotionSource != null)
                                            .Select(x => x.MotionSource)
                                            .Merge()
                                            .Select(messages => messages);

            var motion = detectors.DistinctForTime(TimeSpan.FromMilliseconds(1100));

            //var motion = detectors.Timestamp().Buffer(detectors, (x) => { return Observable.Timer(TimeSpan.FromSeconds(3)); }).Select(x =>
            //                    {
            //                        var vector = new MotionVector();

            //                        foreach (var ev in x)
            //                        {
            //                            if (vector.Path.Count == 0)
            //                            {
            //                                vector.Path.Add(new MotionPoint(ev.Value.MotionDetector, ev.Timestamp));
            //                                continue;
            //                            }

            //                            if (ev.Value.MotionDetector == _motionDetectors[vector.End.MotionDetector].Neighbor)
            //                            {
            //                                vector.Path.Add(new MotionPoint(ev.Value.MotionDetector, ev.Timestamp));
            //                            }
            //                        }

            //                        return vector;
            //                    })
            //                    .Where(y => y.Path.Count > 1)
            //                    .DistinctUntilChanged();

            //var motion = detectors.Timestamp()
            //                      .Buffer(TimeSpan.FromMilliseconds(MOTION_TIME_WINDOW), TimeSpan.FromMilliseconds(MOTION_TIME_SHIFT))
            //                      .Select(x =>
            //                      {
            //                          var vector = new MotionVector();

            //                          foreach (var ev in x)
            //                          {
            //                              if(vector.Path.Count == 0)
            //                              {
            //                                  vector.Path.Add(new MotionPoint(ev.Value.MotionDetector, ev.Timestamp));
            //                                  continue;
            //                              }

            //                              if (ev.Value.MotionDetector == _motionDetectors[vector.End.MotionDetector].Neighbor)
            //                              {
            //                                  vector.Path.Add(new MotionPoint(ev.Value.MotionDetector, ev.Timestamp));
            //                              }
            //                          }

            //                          return vector;
            //                      })
            //                      .Where(y => y.Path.Count > 1)
            //                      .DistinctFor(TimeSpan.FromSeconds(3));

            var resource = motion.Subscribe(x =>
            {
                var time = DateTime.Now;
               Console.WriteLine($"[{time.Minute}:{time.Second}:{time.Millisecond}] {x.MotionDetector.Id}");
                
            });

            AddResourceToDisposeList(resource);
        }
    
        private void AddResourceToDisposeList(IDisposable resource)
        {
            _resources.Add(resource);
        }

        public void Dispose()
        {
            foreach(var resource in _resources)
            {
                resource.Dispose();
            }
        }
    }
}
