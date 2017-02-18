using System;
using System.Collections.Generic;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Sensors;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Triggers;
using System.Reactive.Linq;
using System.Linq;
using System.Diagnostics;

namespace HA4IoT.Extensions
{
    public class LightAutomationService : ILightAutomationService, IService
    {
        private readonly Dictionary<IMotionDetector, MotionDetectorDescriptor> _motionDetectors = new Dictionary<IMotionDetector, MotionDetectorDescriptor>();
        private readonly IAreaService _areaService;

        public LightAutomationService(IAreaService areaService)
        {
            _areaService = areaService;
        }

        public void Startup()
        {
            FindRegistredMotionDetectors();
        }

        public void MonitorArea(IArea area, TimeSpan timeOn)
        {
            throw new NotImplementedException();
        }

        public void ConfigureMotionDetector(IMotionDetector motionDetector, IMotionDetector neighbor, IActuator acutatot)
        {
             if(!_motionDetectors.ContainsKey(motionDetector))
             {
                throw new Exception("This motion detector was not register in any area");
             }

            var descriptor = _motionDetectors[motionDetector];
            descriptor.Neighbor = neighbor;
            descriptor.Acutator = acutatot;

            var trigger = motionDetector.GetMotionDetectedTrigger();

            descriptor.MotionSource = Observable.FromEventPattern<TriggeredEventArgs>(
                                    h => trigger.Triggered += h,
                                    h => trigger.Triggered -= h).Select(x => new MotionDetectorEventArgs { Descriptor = descriptor });
        }
        

        public IDisposable StartWatchForMove()
        {
            return Observable.Merge(_motionDetectors.Values.Select(x => x.MotionSource)).Select(messages => messages).Subscribe(x =>
            {
                Debug.WriteLine(x.Descriptor.MotionDetector.Id);
            });

        }
    
        private void LightAutomationService_Triggered(object sender, Contracts.Triggers.TriggeredEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void FindRegistredMotionDetectors()
        {
            foreach(var area in _areaService.GetAreas())
            {
                foreach(var motionDetector in area.GetComponents<IMotionDetector>())
                {
                    _motionDetectors[motionDetector] = new MotionDetectorDescriptor
                    {
                        Area = area
                    };
                }
            }
        }

        
    }

    public class MotionDetectorDescriptor
    {
        public IMotionDetector MotionDetector { get; set; }

        public IArea Area { get; set; }

        public IMotionDetector Neighbor { get; set; }

        public IActuator Acutator { get; set; }

        public IObservable<MotionDetectorEventArgs> MotionSource { get; set; }
    }

    public class MotionDetectorEventArgs
    {
        public MotionDetectorDescriptor Descriptor;
    }
}
