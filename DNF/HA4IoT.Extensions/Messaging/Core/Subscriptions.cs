using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace HA4IoT.Extensions.Messaging.Core
{
    public class Subscriptions
    {
        private readonly List<Subscription> AllSubscriptions = new List<Subscription>();
        private int _subscriptionRevision;

        private int _localSubscriptionRevision;
        private Subscription[] _localSubscriptions;

        internal Guid RegisterForAsyncResult<T>(Func<IMessageEnvelope<T>, Task> action, MessageFilter filter)
        {
            var type = typeof(T);
            var key = Guid.NewGuid();
            var subscription = new Subscription(type, key, action, filter);

            lock (AllSubscriptions)
            {
                AllSubscriptions.Add(subscription);
                _subscriptionRevision++;
            }

            return key;
        }

        internal Guid Register<T>(Action<IMessageEnvelope<T>> action, MessageFilter filter)
        {
            var type = typeof(T);
            var key = Guid.NewGuid();
            var subscription = new Subscription(type, key, action, filter);

            lock (AllSubscriptions)
            {
                AllSubscriptions.Add(subscription);
                _subscriptionRevision++;
            }

            return key;
        }

        public void UnRegister(Guid token)
        {
            lock (AllSubscriptions)
            {
                var subscription = AllSubscriptions.FirstOrDefault(s => s.Token == token);
                var removed = AllSubscriptions.Remove(subscription);

                if (removed) { _subscriptionRevision++; }
            }
        }

        public void Clear()
        {
            lock (AllSubscriptions)
            {
                AllSubscriptions.Clear();
                _subscriptionRevision++;
            }
        }

        public bool IsRegistered(Guid token)
        {
            lock (AllSubscriptions) { return AllSubscriptions.Any(s => s.Token == token); }
        }

        public Subscription[] GetCurrentSubscriptions()
        {
            if (_localSubscriptions == null)
            {
                _localSubscriptions = new Subscription[0];
            }

            if (_localSubscriptionRevision == _subscriptionRevision)
            {
                return _localSubscriptions;
            }

            Subscription[] latestSubscriptions;
            lock (AllSubscriptions)
            {
                latestSubscriptions = AllSubscriptions.ToArray();
                _localSubscriptionRevision = _subscriptionRevision;
            }

            _localSubscriptions = latestSubscriptions;

            return latestSubscriptions;
        }

        public List<Subscription> GetCurrentSubscriptions(Type messageType, MessageFilter filter = null)
        {
            var latestSubscriptions = GetCurrentSubscriptions();
            var msgTypeInfo = messageType.GetTypeInfo();
            var filteredSubscription = new List<Subscription>();

            for (var idx = 0; idx < latestSubscriptions.Length; idx++)
            {
                var subscription = latestSubscriptions[idx];

                if (!subscription.Type.GetTypeInfo().IsAssignableFrom(msgTypeInfo)) continue;

                if (!subscription.IsFilterMatch(filter)) continue;
                
                filteredSubscription.Add(subscription);
            }

            return filteredSubscription;
        }

      
    }
}
