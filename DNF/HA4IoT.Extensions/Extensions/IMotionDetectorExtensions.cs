using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Sensors;
using System;
using System.Reactive.Linq;


namespace HA4IoT.Extensions
{
    public static class IMotionDetectorExtensions
    {
        public static IObservable<IMotionDetector> ToObservable(this IMotionDetector motionDetector)
        {
            return Observable.FromEventPattern<ComponentFeatureStateChangedEventArgs>
            (
                h => motionDetector.StateChanged += h,
                h => motionDetector.StateChanged -= h
            ).Select(x => motionDetector);
        }
    }
}
