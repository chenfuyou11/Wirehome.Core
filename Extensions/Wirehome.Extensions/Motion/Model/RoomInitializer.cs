using System.Reactive.Concurrency;
using System.Collections.Generic;
using Wirehome.Contracts.Environment;
using Wirehome.Contracts.Core;
using Wirehome.Contracts.Components;

namespace Wirehome.Motion.Model
{
    public class RoomInitializer
    {
        public RoomInitializer(string motionDetectorUid, IEnumerable<string> neighbors, IComponent lamp, AreaDescriptor areaInitializer = null)
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

        public Room ToRoom(MotionConfiguration config, IScheduler scheduler, IDaylightService daylightService, IDateTimeService dateTimeService)
        {
            return new Room(MotionDetectorUid, Neighbors, Lamp, scheduler, daylightService, dateTimeService, AreaInitializer, config);
        }
    }
}