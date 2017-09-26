using System.Collections.Generic;
using Wirehome.Contracts.Services;

namespace Wirehome.Contracts.Components
{
    public interface IComponentRegistryService : IService
    {
        void RegisterComponent(IComponent component);

        IComponent GetComponent(string id);
        
        TComponent GetComponent<TComponent>(string id) where TComponent : IComponent;

        IList<IComponent> GetComponents();

        IList<TComponent> GetComponents<TComponent>() where TComponent : IComponent;
        
        bool ContainsComponent(string id);
    }
}
