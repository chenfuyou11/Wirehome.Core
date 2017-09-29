using System.Threading.Tasks;
using Wirehome.Contracts.Scripting;

namespace Wirehome.Contracts.Services
{
    public abstract class ServiceBase : IService
    {
        public virtual Task Initialize()
        {
            return Task.CompletedTask;
        }

        public virtual IScriptProxy CreateScriptProxy(IScriptingSession scriptingSession, bool x)
        {
            return null;
        }
    }
}
