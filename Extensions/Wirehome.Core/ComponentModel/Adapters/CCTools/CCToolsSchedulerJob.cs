using Quartz;
using System.Threading.Tasks;
using Wirehome.Core.Services.Quartz;

namespace Wirehome.ComponentModel.Adapters
{
    public class CCToolsSchedulerJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            var adapter = context.GetDataContext<CCToolsBaseAdapter>();
            return adapter.FetchState();
        }
    }
}
