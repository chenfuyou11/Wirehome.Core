using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wirehome.ComponentModel.Commands;
using Wirehome.ComponentModel.Events;
using Wirehome.Core.EventAggregator;
using System.Linq;

namespace Wirehome.ComponentModel.Extensions
{
    public static class EventAggregatorExtensions
    {
        public static Task<R> QueryDeviceAsync<T, R>(this IEventAggregator eventAggregator, T message) where T : BaseObject
                                                                                                       where R : BaseObject
        {
            return eventAggregator.QueryAsync<T, R>(message, new RoutingFilter(message?[CommandProperties.DeviceUid]?.ToString() ?? string.Empty));
        }

        public static IDisposable SubscribeForDeviceQuery<T>(this IEventAggregator eventAggregator, Func<IMessageEnvelope<T>, Task<object>> action, string uid) where T : BaseObject
        {
            return eventAggregator.SubscribeForAsyncResult<T>(action, new RoutingFilter(uid));
        }

        public static IDisposable SubscribeForDeviceEvent<T>(this IEventAggregator eventAggregator, Func<IMessageEnvelope<T>, Task> action, string uid, IDictionary<string, string> attributes) where T : BaseObject
        {
            return eventAggregator.SubscribeAsync<T>(action, new RoutingFilter(uid, attributes));
        }

        public static Task PublishDeviceEvent<T>(this IEventAggregator eventAggregator, T message) where T : Event
        {
            return eventAggregator.Publish(message, new RoutingFilter(message[EventProperties.SourceDeviceUid].ToString()));
        }

        public static Task PublishDeviceEvent<T>(this IEventAggregator eventAggregator, T message, IList<string> attributes) where T : Event
        {
            return eventAggregator.Publish(message, new RoutingFilter(message[EventProperties.SourceDeviceUid].ToString(),
                attributes.ToDictionary(k => k, v => message.Properties[v].ToString())));
        }

        public static Task PublishDeviceCommnd<T>(this IEventAggregator eventAggregator, T message) where T : Command
        {
            return eventAggregator.Publish(message, new RoutingFilter(message[CommandProperties.DeviceUid].ToString()));
        }
    }
}
