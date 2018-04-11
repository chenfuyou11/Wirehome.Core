using System.Collections.Generic;
using System.Text;

namespace Wirehome.Contracts.Scripting
{
    public interface IScriptingSession
    {
        StringBuilder DebugOutput { get; }

        IList<IScriptProxy> Proxies { get; }

        ScriptExecutionResult Execute(string entryFunctionName = null);
    }
}
