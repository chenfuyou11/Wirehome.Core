using System.Reactive.Concurrency;
using System.Collections.Generic;
using Wirehome.Contracts.Environment;
using Wirehome.Contracts.Core;
using Wirehome.Contracts.Components;

namespace Wirehome.Extensions.MotionModel
{
    public class MotionDesctiptorInitializer
    {
        public MotionDesctiptorInitializer(string motionDetectorUid, IEnumerable<string> neighbors, IComponent lamp, AreaDescriptor areaInitializer = null)
        {
            MotionDetectorUid = motionDetectorUid;
            Neighbors = neighbors;
            Lamp = lamp;
            AreaInitializer = areaInitializer;
        }

        public string MotionDetectorUid { get; }
        public IEnumerable<string> Neighbors { get; }
        public IComponent Lamp { get; }
        public AreaDescriptor AreaInitializer { get; }

        public MotionDescriptor ToMotionDescriptor(MotionConfiguration config, IScheduler scheduler, IDaylightService daylightService, IDateTimeService dateTimeService)
        {
            return new MotionDescriptor(MotionDetectorUid, Neighbors, Lamp, scheduler, daylightService, dateTimeService, AreaInitializer, config);
        }
    }
}