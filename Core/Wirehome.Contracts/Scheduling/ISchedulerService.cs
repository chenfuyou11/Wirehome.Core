using System;
using System.Threading.Tasks;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Contracts.Scheduling
{
    public interface ISchedulerService : IService
    {
        void Register(string name, TimeSpan interval, Action action, bool isOneTimeSchedule = false);

        void Register(string name, TimeSpan interval, Func<Task> action, bool isOneTimeSchedule = false);

        void Remove(string name);
    }
}
