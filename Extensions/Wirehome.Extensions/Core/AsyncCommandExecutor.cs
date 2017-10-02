using System;
using Wirehome.Contracts.Components.Commands;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wirehome.Extensions.Extensions;

namespace Wirehome.Extensions.Devices
{
    public class AsyncCommandExecutor
    {
        private readonly Dictionary<Type, object> _actions = new Dictionary<Type, object>();

        public void Register<T>(Func<T, Task> callback) where T : ICommand
        {
            if (callback == null) throw new ArgumentNullException(nameof(callback));
            _actions.Add(typeof(T), callback);
        }

        public Task Execute<T>(T command = default) where T : ICommand
        {
            return (_actions.ElementAtOrNull(typeof(T)) as Func<T, Task>)?.Invoke(command) ?? Task.CompletedTask;
        }
    }

}
