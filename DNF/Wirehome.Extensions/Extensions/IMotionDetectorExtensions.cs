using Wirehome.Contracts.Components;
using Wirehome.Contracts.Sensors;
using System;
using System.Reactive.Linq;


namespace Wirehome.Extensions
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
