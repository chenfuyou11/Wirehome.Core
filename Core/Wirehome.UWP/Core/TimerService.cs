using System;
using System.Diagnostics;
using System.Threading;
using Windows.System.Threading;
using Wirehome.Contracts.Core;
using Wirehome.Contracts.Logging;
using Wirehome.Contracts.Services;

namespace Wirehome.Core
{
    public sealed class TimerService : ServiceBase, ITimerService
    {
        private readonly TimerTickEventArgs _timerTickEventArgs = new TimerTickEventArgs();
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
        private readonly ILogger _log;
        
        private int _runningThreads;

        public TimerService(ILogService logService)
        {
            _log = logService?.CreatePublisher(nameof(TimerService)) ?? throw new ArgumentNullException(nameof(logService));
            
            ThreadPoolTimer.CreatePeriodicTimer(TickInternal, TimeSpan.FromMilliseconds(50));
        }

        public event EventHandler<TimerTickEventArgs> Tick;

        public void CreatePeriodicTimer(Action action, TimeSpan period)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            ThreadPoolTimer.CreatePeriodicTimer((timer) => { action(); }, period);
        }

        private void TickInternal(object state)
        {
            try
            {
                if (Interlocked.Increment(ref _runningThreads) > 1)
                {
                    return;
                }
                
                _stopwatch.Stop();
                _timerTickEventArgs.ElapsedTime = _stopwatch.Elapsed;
                _stopwatch.Restart();

                if (_timerTickEventArgs.ElapsedTime.TotalMilliseconds > 1000)
                {
                    _log.Warning($"Tick took {_timerTickEventArgs.ElapsedTime.TotalMilliseconds}ms.");
                }

                Tick?.Invoke(this, _timerTickEventArgs);
            }
            catch (Exception exception)
            {
                _log.Error(exception, "Tick has catched an unhandled exception.");
            }
            finally
            {
                Interlocked.Decrement(ref _runningThreads);
            }
        }
    }
}