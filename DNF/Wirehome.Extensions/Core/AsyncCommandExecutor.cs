using System;
using Wirehome.Contracts.Components.Commands;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wirehome.Extensions.Extensions;

namespace Wirehome.Extensions.Devices
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
