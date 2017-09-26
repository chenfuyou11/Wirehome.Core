using System;
using System.Threading.Tasks;
using Wirehome.Contracts.Services;

namespace Wirehome.Contracts.Scheduling
{
    public interface ISchedulerService : IService
    {
        void Register(string name, TimeSpan interval, Action action, bool isOneTimeSchedule = false);

        void Register(string name, TimeSpan interval, Func<Task> action, bool isOneTimeSchedule = false);

        void Remove(string name);
    }
}
