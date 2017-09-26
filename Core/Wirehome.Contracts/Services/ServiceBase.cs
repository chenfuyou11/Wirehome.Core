using Wirehome.Contracts.Scripting;

namespace Wirehome.Contracts.Services
{
    public abstract class ServiceBase : IService
    {
        public virtual void Startup()
        {
        }

        public virtual IScriptProxy CreateScriptProxy(IScriptingSession scriptingSession, bool x)
        {
            return null;
        }
    }
}
