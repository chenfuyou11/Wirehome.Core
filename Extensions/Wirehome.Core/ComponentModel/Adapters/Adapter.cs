using System.Threading.Tasks;
using Wirehome.ComponentModel;
using Wirehome.Core;

namespace Wirehome.ComponentModel.Adapters
{
    public abstract class Adapter : BaseObject, IService
    {
        protected readonly DisposeContainer _disposables = new DisposeContainer();

        public void Dispose() => _disposables.Dispose();
        
        public abstract Task Initialize();
       
    }
}