﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wirehome.ComponentModel.Commands;
using Wirehome.ComponentModel.Events;
using Wirehome.Core.EventAggregator;
using System.Linq;
using Wirehome.Model.Extensions;

namespace Wirehome.ComponentModel.Extensions
{
    public static class EventAggregatorExtensions
    {
        public static Task<R> QueryDeviceAsync<R>(this IEventAggregator eventAggregator, DeviceCommand message) where R : BaseObject
        {
            return eventAggregator.QueryAsync<DeviceCommand, R>(message, new RoutingFilter(message?[CommandProperties.DeviceUid]?.ToString() ?? string.Empty));
        }

        public static Task<R> QueryDeviceAsync<R>(this IEventAggregator eventAggregator, DeviceCommand message, TimeSpan timeOut) where R : BaseObject
        {
            return eventAggregator.QueryWitTimeoutAsync<DeviceCommand, R>(message, timeOut, new RoutingFilter(message?[CommandProperties.DeviceUid]?.ToString() ?? string.Empty));
        }

        public static IDisposable SubscribeForDeviceQuery<T>(this IEventAggregator eventAggregator, Func<IMessageEnvelope<T>, Task<object>> action, string uid) where T : BaseObject
        {
            return eventAggregator.SubscribeForAsyncResult<T>(action, new RoutingFilter(uid));
        }

        public static IDisposable SubscribeForDeviceEvent(this IEventAggregator eventAggregator, Func<IMessageEnvelope<Event>, Task> action, IDictionary<string, string> attributes, string eventType = EventType.PropertyChanged)
        {
            var routingKey = attributes[EventProperties.SourceDeviceUid];
            attributes.Add(EventProperties.EventType, eventType);

            return eventAggregator.SubscribeAsync(action, new RoutingFilter(routingKey, attributes));
        }

        public static Task PublishDeviceEvent<T>(this IEventAggregator eventAggregator, T message) where T : Event
        {
            var routingAttributes = message.RoutingAttributes();
            if(routingAttributes != null)
            {
                return PublishDeviceEvent(eventAggregator, message, routingAttributes);
            }

            return eventAggregator.Publish(message, new RoutingFilter(message[EventProperties.SourceDeviceUid].ToString()));
        }

        public static Task PublishDeviceEvent<T>(this IEventAggregator eventAggregator, T message, IEnumerable<string> routerAttributes) where T : Event
        {
            var routing = routerAttributes.ToDictionary(k => k, v => message[v].ToString());

            routing.Add(EventProperties.SourceDeviceUid, message[EventProperties.SourceDeviceUid].ToString());
            routing.Add(EventProperties.EventType, message.Type);

            return eventAggregator.Publish(message, new RoutingFilter(message[EventProperties.SourceDeviceUid].ToString(), routing));
        }

        public static Task PublishDeviceCommnd<T>(this IEventAggregator eventAggregator, T message) where T : Command
        {
            return eventAggregator.Publish(message, new RoutingFilter(message[CommandProperties.DeviceUid].ToString()));
        }
    }
}