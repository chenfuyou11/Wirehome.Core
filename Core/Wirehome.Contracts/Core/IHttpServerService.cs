using HTTPnet.Core.Pipeline;
using Wirehome.Contracts.Services;

namespace Wirehome.Contracts.Core
{
    public interface IHttpServerService : IService
    {
        void AddRequestHandler(IHttpContextPipelineHandler handler);
    }
}
