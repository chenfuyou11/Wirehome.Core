using System.Reactive.Concurrency;
using System.Collections.Generic;
using Wirehome.Contracts.Environment;
using Wirehome.Contracts.Core;
using Wirehome.Contracts.Logging;

namespace Wirehome.Motion.Model
{
    public class RoomInitializer
    {
        public RoomInitializer(string motionDetectorUid, IEnumerable<string> neighbors, IMotionLamp lamp, IEnumerable<IEventDecoder> eventDecoders, AreaDescriptor areaInitializer = null)
        {
            MotionDetectorUid = motionDetectorUid;
            Neighbors = neighbors;
            Lamp = lamp;
            AreaInitializer = areaInitializer;
            EventDecoders = eventDecoders;
        }

        public string MotionDetectorUid { get; }
        public IEnumerable<string> Neighbors { get; }
        public IMotionLamp Lamp { get; }
        public AreaDescriptor AreaInitializer { get; }
        public IEnumerable<IEventDecoder> EventDecoders { get; }

        public Room ToRoom(MotionConfiguration config, IDaylightService daylightService, 
                           IDateTimeService dateTimeService, IConcurrencyProvider concurrencyProvider, ILogger logger)
        {
            return new Room(MotionDetectorUid, Neighbors, Lamp, daylightService, dateTimeService, concurrencyProvider, logger, AreaInitializer, config, EventDecoders);
        }
    }
}


