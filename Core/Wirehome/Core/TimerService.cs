using System;
using System.Diagnostics;
using System.Threading;
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
        private readonly INativeTimerSerice _nativeTimerSerice;

        public TimerService(ILogService logService, INativeTimerSerice nativeTimerSerice)
        {
            _log = logService?.CreatePublisher(nameof(TimerService)) ?? throw new ArgumentNullException(nameof(logService));
            _nativeTimerSerice = nativeTimerSerice ?? throw new ArgumentNullException(nameof(nativeTimerSerice));

            _nativeTimerSerice.CreatePeriodicTimer(TickInternal, TimeSpan.FromMilliseconds(50));
        }

        public event EventHandler<TimerTickEventArgs> Tick;
          

        private void TickInternal()
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