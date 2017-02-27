using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;


namespace HA4IoT.Extensions.Extensions
{
    public static class ReactiveExtensions
    {
        public static IObservable<T> DistinctFor<T>(this IObservable<T> src, TimeSpan validityPeriod)
        {
            var hs = new Dictionary<EqualityDecorator<T>, EqualityDecorator<T>>();

            return src.Select(item => new EqualityDecorator<T>(item)).Where(df =>
            {
                if (hs.TryGetValue(df, out EqualityDecorator<T> hsVal))
                {
                    var age = DateTime.UtcNow - hsVal.Created;
                    if (age < validityPeriod)
                    {
                        return false;
                    }
                }
                hs[df] = df;
                return true;

            }).Select(df => df.Item);
        }

        public static IObservable<T> Pace<T>(this IObservable<T> src, TimeSpan delay)
        {
            var timer = Observable.Timer(TimeSpan.FromSeconds(0), delay);

            return src.Zip(timer, (s, t) => s);
        }
    }

   
}
