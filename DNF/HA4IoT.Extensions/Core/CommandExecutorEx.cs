using HA4IoT.Components;
using HA4IoT.Components.Commands;
using HA4IoT.Contracts.Components.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HA4IoT.Extensions.Core
{
    public class CommandExecutorEx
    {
        private readonly Dictionary<Type, ICommandExecutorAction> _actions = new Dictionary<Type, ICommandExecutorAction>();

        public void Register<TCommand>() where TCommand : ICommand
        {
            _actions.Add(typeof(TCommand), new CommandExecutorAction<TCommand>(c => { }));
        }

        public void Register<TCommand>(Action<TCommand> callback) where TCommand : ICommand
        {
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            _actions.Add(typeof(TCommand), new CommandExecutorAction<TCommand>(callback));
        }

        public void Execute(ICommand command)
        {
            ICommandExecutorAction action;
            var commandType = command.GetType();
            if (!_actions.TryGetValue(commandType, out action))
            {
                foreach(var key in _actions.Keys)
                {
                    if(key.GetTypeInfo().IsSubclassOf(commandType))
                    {
                        action = _actions[key];
                        break;
                    }
                }

                if (action == null)
                {
                    throw new CommandNotSupportedException(command);
                }
            }

            action.Execute(command);
        }
    }
}
