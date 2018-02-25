namespace Wirehome.Core.Adapters.CCTools
{
    public abstract class Adapter : BaseObject
    {
        protected readonly DisposeContainer _disposables = new DisposeContainer();

        public abstract void Initialize();   
    }
}