using HA4IoT.Contracts.Sensors;
using HA4IoT.Contracts.Triggers;
using System;
using System.Reactive.Linq;


namespace HA4IoT.Extensions
{
    public static class IMotionDetectorExtensions
    {
        public static IObservable<IMotionDetector> ToObservable(this IMotionDetector motionDetector)
        {
            return Observable.FromEventPattern<TriggeredEventArgs>
            (
                h => motionDetector.MotionDetectedTrigger.Triggered += h,
                h => motionDetector.MotionDetectedTrigger.Triggered -= h
            ).Select(x => motionDetector);
        }
    }
}
