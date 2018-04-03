using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Wirehome.ComponentModel.Adapters;
using Wirehome.ComponentModel.Commands;
using Wirehome.Core;
using Wirehome.Core.Extensions;

namespace Wirehome.ComponentModel.Components
{
    public abstract class Actor : BaseObject, IService
    {
        protected bool _isInitialized;

        protected BufferBlock<CommandJob<object>> _commandQueue = new BufferBlock<CommandJob<object>>();
        protected readonly DisposeContainer _disposables = new DisposeContainer();
        protected Dictionary<string, Func<Command, Task<object>>> _asyncQueryHandlers = new Dictionary<string, Func<Command, Task<object>>>();
        protected Dictionary<string, Func<Command, Task>> _asyncCommandHandlers = new Dictionary<string, Func<Command, Task>>();
        protected Dictionary<string, Action<Command>> _commandHandlers = new Dictionary<string, Action<Command>>();

        public void Dispose() => _disposables.Dispose();

        public virtual async Task Initialize()
        {
            HandleCommands();

            _isInitialized = true;
        }

        public Actor() => RegisterCommandHandlers();

        private void RegisterCommandHandlers()
        {
            Regex r = new Regex(@"^(?<Name>\w*)Handler", RegexOptions.Compiled);
            RegisterAsyncQueryHandlers(r);
            RegisterAsyncCommandsHandlers(r);
            RegisterActionHandlers(r);
        }

        private void RegisterAsyncQueryHandlers(Regex r)
        {
            foreach (var handler in GetType().GetMethodsBySignature(typeof(Task<object>), typeof(Command)))
            {
                var command = GetCommandName(r, handler);
                if (command.HasValue)
                {
                    _asyncQueryHandlers.Add(command.Value, (Func<Command, Task<object>>)Delegate.CreateDelegate(typeof(Func<Command, Task<object>>), this, handler, false));
                }
            }
        }

        private void RegisterAsyncCommandsHandlers(Regex r)
        {
            foreach (var handler in GetType().GetMethodsBySignature(typeof(Task), typeof(Command)))
            {
                var command = GetCommandName(r, handler);
                if (command.HasValue)
                {
                    _asyncCommandHandlers.Add(command.Value, (Func<Command, Task>)Delegate.CreateDelegate(typeof(Func<Command, Task>), this, handler, false));
                }
            }
        }

        private void RegisterActionHandlers(Regex r)
        {
            foreach (var handler in GetType().GetMethodsBySignature(typeof(void), typeof(Command)))
            {
                var command = GetCommandName(r, handler);
                if (command.HasValue)
                {
                    _commandHandlers.Add(command.Value, (Action<Command>)Delegate.CreateDelegate(typeof(Action<Command>), this, handler, false));
                }
            }
        }

        private Maybe<string> GetCommandName(Regex r, System.Reflection.MethodInfo handler)
        {
            var match = r.Match(handler.Name);
            if (match?.Success ?? false)
            {
                return match.Groups["Name"].Value;
            }
            return Maybe<string>.None;
        }

        private async Task HandleCommands()
        {
            while (await _commandQueue.OutputAvailableAsync(_disposables.Token))
            {
                try
                {
                    var command = await _commandQueue.ReceiveAsync(_disposables.Token);
                    var result = await ProcessCommand(command.Command);
                    AssertForWrappedTask(result);
                    command.SetResult(result);
                }
                catch (Exception ex)
                {
                    LogException(ex);
                }
            }
        }

        protected abstract void LogException(Exception ex);

        private static void AssertForWrappedTask(object result)
        {
            if (result.GetType().Namespace == "System.Threading.Tasks") throw new Exception("Result from handler wan not unwrapped properly");
        }

        public Task<object> ExecuteCommand(Command command)
        {
            if (!_isInitialized) throw new Exception($"Component {Uid} is not initialized");
            return QueueJob(command).Unwrap();
        }

        public Task<T> ExecuteCommand<T>(Command command) => ExecuteCommand(command).Cast<T>();

        private async Task<Task<object>> QueueJob(Command command)
        {
            var commandJob = new CommandJob<object>(command);
            var sendResult = await _commandQueue.SendAsync(commandJob);
            return commandJob.Result;
        }

        private Task<object> ProcessCommand(Command command)
        {
            if (_asyncQueryHandlers.ContainsKey(command.Type))
            {
                return _asyncQueryHandlers?[command.Type]?.Invoke(command);
            }
            else if (_asyncCommandHandlers.ContainsKey(command.Type))
            {
                return _asyncCommandHandlers?[command.Type]?.Invoke(command).Cast<object>(0);
            }
            else if (_commandHandlers.ContainsKey(command.Type))
            {
                _commandHandlers?[command.Type]?.Invoke(command);
                return Task.FromResult<object>(0);
            }
            else
            {
                return UnhandledCommand(command);
            }
        }

        protected virtual Task<object> UnhandledCommand(Command command)
        {
            throw new Exception($"Component [{Uid}] cannot process command because there is no registered handler for [{command.Type}]");
        }
    }
}