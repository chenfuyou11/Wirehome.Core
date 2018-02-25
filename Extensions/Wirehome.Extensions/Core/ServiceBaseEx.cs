using System;
using System.Threading.Tasks;
using Wirehome.Contracts.Scripting;
using Wirehome.Contracts.Services;
using Wirehome.Core;

namespace Wirehome.Extensions.Core
{
    public class ServiceBaseEx : ServiceBase, IDisposable
    {
        protected readonly DisposeContainer _disposeContainer = new DisposeContainer();

        public void Dispose()
        {
            _disposeContainer.Dispose();
        }
    }
}
