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
        private bool _isInitialized;

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
                var command = await _commandQueue.ReceiveAsync(_disposables.Token);
                var result = await ProcessCommand(command.Command);
                command.SetResult(result);
            }
        }

        public async Task<object> ExecuteCommand(Command command, CancellationToken callerCancelationToken = default)
        {
            if (!_isInitialized) throw new Exception($"Component {Uid} is not initialized");

            var commandJob = new CommandJob<object>(command);
            var sendResult = await _commandQueue.SendAsync(commandJob, CancellationTokenSource.CreateLinkedTokenSource(callerCancelationToken, _disposables.Token).Token);
            //TODO Test for exceptions
            if (sendResult)
            {
                return commandJob.Result;
            }
            return null;
        }

        private Task<object> ProcessCommand(Command message)
        {
            if (_asyncQueryHandlers.ContainsKey(message.Type))
            {
                return _asyncQueryHandlers?[message.Type]?.Invoke(message);
            }
            else if (_asyncCommandHandlers.ContainsKey(message.Type))
            {
                return _asyncCommandHandlers?[message.Type]?.Invoke(message).ToEmptyResultTask();
            }
            else if (_commandHandlers.ContainsKey(message.Type))
            {
                _commandHandlers?[message.Type]?.Invoke(message);
                return Task.FromResult<object>(0);
            }

            throw new Exception($"Component {Uid} cannot process command because there is no registered handler for {message.Type}");
        }
    }
}