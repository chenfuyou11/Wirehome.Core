using System;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Sensors;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Extensions.MotionModel
{
    public class MotionDescriptor
    {
        public IMotionDetector MotionDetector { get; set; }

        public IArea Area { get; set; }

        public IMotionDetector Neighbor { get; set; }

        public IComponent Acutator { get; set; }

        public IObservable<MotionDescriptor> MotionSource { get; set; }
    }
}
