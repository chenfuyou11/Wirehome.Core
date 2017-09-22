using System;
using HA4IoT.Contracts.Components.Commands;
using System.Collections.Generic;
using System.Threading.Tasks;
using HA4IoT.Extensions.Extensions;

namespace HA4IoT.Extensions.Devices
{
    public class AsyncCommandExecutor
    {
        private readonly Dictionary<Type, Func<ICommand, Task>> _actions = new Dictionary<Type, Func<ICommand, Task>>();

        public void Register<T>(Func<ICommand, Task> callback)
        {
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            _actions.Add(typeof(T), callback);
        }

        public Task Execute<T>()
        {
            return _actions.ElementAtOrNull(typeof(T))?.Invoke(null);
        }
    }

}
