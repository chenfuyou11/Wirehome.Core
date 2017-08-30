using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HA4IoT.Extensions.Extensions
{
    public static class TaskExtensions
    {
        public static async Task<Task> WhenAll(this IEnumerable<Task> tasks, int millisecondsTimeOut, CancellationToken cancellationToken) 
        {
            var timeoutTask = Task.Delay(millisecondsTimeOut, cancellationToken);
            var workTasks = Task.WhenAll(tasks);

            var result = await Task.WhenAny(new[] { timeoutTask, workTasks }).ConfigureAwait(false);

            if (result == timeoutTask)
            {
                if (cancellationToken.IsCancellationRequested && timeoutTask.Status == TaskStatus.Canceled)
                {
                    throw new OperationCanceledException();
                }
                if (timeoutTask.Status == TaskStatus.RanToCompletion && !cancellationToken.IsCancellationRequested)
                {
                    throw new TimeoutException();
                }

                throw new InvalidOperationException("Not supported result in WhenAll");
            }
            if(result == workTasks && result.IsFaulted)
            {
                var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                tokenSource.Cancel();
            }
            
            return result;
        }

        public static async Task<R> WhenAny<R>(this IEnumerable<Task> tasks, int millisecondsTimeOut, CancellationToken cancellationToken) where R : class
        {
            var timeoutTask = Task.Delay(millisecondsTimeOut, cancellationToken);
            var result = await Task.WhenAny(tasks.ToList().AddChained(timeoutTask)).ConfigureAwait(false);

            if (result == timeoutTask)
            {
                if (cancellationToken.IsCancellationRequested && timeoutTask.Status == TaskStatus.Canceled)
                {
                    throw new OperationCanceledException();
                }
                if (timeoutTask.Status == TaskStatus.RanToCompletion && !cancellationToken.IsCancellationRequested)
                {
                    throw new TimeoutException();
                }

                throw new InvalidOperationException("Not supported result in WhenAll");
            }

            return (result as Task<R>)?.Result ?? throw new InvalidCastException($"Excepted type {typeof(R)} is diffrent that actual");
        }

        public static Task WhenCanceled(this CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).SetResult(true), tcs);
            return tcs.Task;
        }

        public static Task ForEachAsync<T>(this IEnumerable<T> source, Action<T> body, CancellationToken token = default(CancellationToken))
        {
            return Task.WhenAll(from item in source select Task.Run(() => body(item), token));
        }

    }
}
