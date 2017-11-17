using HTTPnet.Core.Pipeline;

namespace Wirehome.Contracts.Core
{
    public interface IHttpServerService
    {
        void AddRequestHandler(IHttpContextPipelineHandler handler);
    }
}
