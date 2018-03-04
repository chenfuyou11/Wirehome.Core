using Quartz;
using Wirehome.Core;
using Wirehome.Core.Communication.I2C;
using Wirehome.Core.EventAggregator;

namespace Wirehome.ComponentModel.Adapters
{
    public interface IAdapterServiceFactory
    {
        IEventAggregator GetEventAggregator();
        II2CBusService GetI2CService();
        ILogger GetLogger();
        IScheduler GetScheduler();
    }
}