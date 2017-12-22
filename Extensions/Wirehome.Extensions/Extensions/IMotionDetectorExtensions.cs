using Wirehome.Contracts.Components;
using Wirehome.Contracts.Sensors;
using System;
using Wirehome.Contracts.Components.States;
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

        public static IObservable<PowerStateValue> ToPowerChangeSource(this IComponent lamp)
        {
            return Observable.FromEventPattern<ComponentFeatureStateChangedEventArgs>
            (
                h => lamp.StateChanged += h,
                h => lamp.StateChanged -= h
            ).Where(y => y.EventArgs?.NewState != null)
            .Select(x => x.EventArgs.NewState.Extract<PowerState>().Value);
        }
    }
}
