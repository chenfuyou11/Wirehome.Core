using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Wirehome.Contracts.Api;
using Wirehome.Contracts.Core;
using Wirehome.Contracts.Logging;
using Wirehome.Contracts.Scheduling;
using Wirehome.Contracts.Scripting;
using Wirehome.Contracts.Services;
using Wirehome.Core;
using Newtonsoft.Json.Linq;

namespace Wirehome.Scheduling
{
    [ApiServiceClass(typeof(ISchedulerService))]
    public class SchedulerService : ServiceBase, ISchedulerService
    {
        private readonly List<Schedule> _schedules = new List<Schedule>();
        private readonly IDateTimeService _dateTimeService;
        private readonly INativeTimerSerice _nativeTimerSerice;
        private readonly ILogger _log;

        public SchedulerService(IDateTimeService dateTimeService, ILogService logService, IScriptingService scriptingService, INativeTimerSerice nativeTimerSerice)
        {
            if (scriptingService == null) throw new ArgumentNullException(nameof(scriptingService));
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
            _nativeTimerSerice = nativeTimerSerice ?? throw new ArgumentNullException(nameof(nativeTimerSerice));
            _log = logService?.CreatePublisher(nameof(SchedulerService)) ?? throw new ArgumentNullException(nameof(logService));
            scriptingService.RegisterScriptProxy(s => new SchedulerScriptProxy(this, s));

            _nativeTimerSerice.CreatePeriodicTimer(ExecuteSchedules, TimeSpan.FromMilliseconds(250));
        }

        [ApiMethod]
        public void GetSchedules(IApiCall apiCall)
        {
            lock (_schedules)
            {
                apiCall.Result = JObject.FromObject(_schedules);
            }
        }

        public void Register(string name, TimeSpan interval, Action action, bool isOneTimeSchedule = false)
        {
            Register(name, interval, () =>
            {
                action();
                return Task.FromResult(0);
            });
        }

        public void Register(string name, TimeSpan interval, Func<Task> action, bool isOneTimeSchedule = false)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (action == null) throw new ArgumentNullException(nameof(action));

            lock (_schedules)
            {
                if (_schedules.Any(s => s.Name.Equals(name)))
                {
                    throw new InvalidOperationException($"Schedule with name '{name}' is already registered.");
                }

                var schedule = new Schedule(name, interval, action) { NextExecution = _dateTimeService.Now, IsOneTimeSchedule = isOneTimeSchedule };
                _schedules.Add(schedule);

                _log.Info($"Registerd schedule '{name}' with interval of {interval}.");
            }
        }

        public void Remove(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            lock (_schedules)
            {
                _schedules.RemoveAll(s => s.Name.Equals(name));

                _log.Info($"Removed schedule '{name}'.");
            }
        }

        private void ExecuteSchedules()
        {
            var now = _dateTimeService.Now;

            lock (_schedules)
            {
                for (var i = _schedules.Count - 1; i >= 0; i--)
                {
                    var schedule = _schedules[i];

                    if (schedule.Status == ScheduleStatus.Running || now < schedule.NextExecution)
                    {
                        continue;
                    }

                    Task.Run(() => TryExecuteScheduleAsync(schedule));

                    if (schedule.IsOneTimeSchedule)
                    {
                        _schedules.RemoveAt(i);
                    }
                }
            }
        }

        private async Task TryExecuteScheduleAsync(Schedule schedule)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _log.Verbose($"Executing schedule '{schedule.Name}'.");
                schedule.Status = ScheduleStatus.Running;

                await schedule.Action();

                schedule.LastErrorMessage = null;
                schedule.Status = ScheduleStatus.Idle;
            }
            catch (Exception exception)
            {
                _log.Error(exception, $"Error while executing schedule '{schedule.Name}'.");

                schedule.Status = ScheduleStatus.Faulted;
                schedule.LastErrorMessage = exception.Message;
            }
            finally
            {
                schedule.LastExecutionDuration = stopwatch.Elapsed;
                schedule.LastExecution = _dateTimeService.Now;
                schedule.NextExecution = schedule.LastExecution.Value + schedule.Interval;
            }
        }
    }
}
