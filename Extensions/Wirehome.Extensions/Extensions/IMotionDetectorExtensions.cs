using Wirehome.Contracts.Components;
using Wirehome.Contracts.Sensors;
using System;
using System.Reactive.Linq;
using Wirehome.Components;
using Wirehome.Contracts.Components.States;
using Wirehome.Contracts.Actuators;

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

        public static IObservable<PowerStateValue> ToPowerChangeSource(this ILamp lamp)
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
