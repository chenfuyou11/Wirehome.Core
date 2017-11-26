using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Wirehome.Extensions.Messaging.Core
{
    public class Subscriptions
    {
        private readonly List<BaseCommandHandler> _allSubscriptions = new List<BaseCommandHandler>();
        private int _subscriptionRevision;

        private int _localSubscriptionRevision;
        private BaseCommandHandler[] _localSubscriptions;

        internal Guid RegisterForAsyncResult<T>(Func<IMessageEnvelope<T>, Task> action, MessageFilter filter)
        {
            var type = typeof(T);
            var key = Guid.NewGuid();
            var subscription = new AsyncCommandHandler(type, key, action, filter);

            lock (_allSubscriptions)
            {
                _allSubscriptions.Add(subscription);
                _subscriptionRevision++;
            }

            return key;
        }

        internal Guid Register<T>(Action<IMessageEnvelope<T>> action, MessageFilter filter)
        {
            var type = typeof(T);
            var key = Guid.NewGuid();
            var subscription = new CommandHandler(type, key, action, filter);

            lock (_allSubscriptions)
            {
                _allSubscriptions.Add(subscription);
                _subscriptionRevision++;
            }

            return key;
        }

        public void UnRegister(Guid token)
        {
            lock (_allSubscriptions)
            {
                var subscription = _allSubscriptions.FirstOrDefault(s => s.Token == token);
                var removed = _allSubscriptions.Remove(subscription);

                if (removed) { _subscriptionRevision++; }
            }
        }

        public void Clear()
        {
            lock (_allSubscriptions)
            {
                _allSubscriptions.Clear();
                _subscriptionRevision++;
            }
        }

        public bool IsRegistered(Guid token)
        {
            lock (_allSubscriptions) { return _allSubscriptions.Any(s => s.Token == token); }
        }

        public BaseCommandHandler[] GetCurrentSubscriptions()
        {
            if (_localSubscriptions == null)
            {
                _localSubscriptions = new BaseCommandHandler[0];
            }

            if (_localSubscriptionRevision == _subscriptionRevision)
            {
                return _localSubscriptions;
            }

            BaseCommandHandler[] latestSubscriptions;
            lock (_allSubscriptions)
            {
                latestSubscriptions = _allSubscriptions.ToArray();
                _localSubscriptionRevision = _subscriptionRevision;
            }

            _localSubscriptions = latestSubscriptions;

            return latestSubscriptions;
        }

        public List<BaseCommandHandler> GetCurrentSubscriptions(Type messageType, MessageFilter filter = null)
        {
            var latestSubscriptions = GetCurrentSubscriptions();
            var msgTypeInfo = messageType.GetTypeInfo();
            var filteredSubscription = new List<BaseCommandHandler>();

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
