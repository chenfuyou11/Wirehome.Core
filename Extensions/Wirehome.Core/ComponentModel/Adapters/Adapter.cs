using System.Collections.Generic;
using System.Threading.Tasks;
using Wirehome.Core;

namespace Wirehome.ComponentModel.Adapters
{
    public abstract class Adapter : BaseObject, IService
    {
        protected readonly DisposeContainer _disposables = new DisposeContainer();
        protected readonly List<string> _requierdProperties = new List<string>();

        public void Dispose() => _disposables.Dispose();

        public abstract Task Initialize();
        public IList<string> RequierdProperties() => _requierdProperties;
    }
}