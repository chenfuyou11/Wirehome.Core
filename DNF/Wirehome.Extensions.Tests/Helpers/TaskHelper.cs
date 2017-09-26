using System.Threading;
using System.Threading.Tasks;

namespace Wirehome.Extensions.Tests.Helpers
{
    public static class TaskHelper
    {
        public static TaskCompletionSource<bool> GenerateTimeoutTaskSource()
        {
            var ts = new CancellationTokenSource(millisecondsDelay: 1000);
            var tcs = new TaskCompletionSource<bool>();
            ts.Token.Register(() => tcs.TrySetCanceled(), useSynchronizationContext: false);
            return tcs;
        }
    }
}
