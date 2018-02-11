using System.Collections.Generic;
using Wirehome.Contracts.Components;
using Wirehome.Contracts.Services;

namespace Wirehome.Extensions.Contracts
{
    public interface IAlexaDispatcherService : IService
    {
        void RegisterDevice(string friendlyName, IEnumerable<IComponent> devices);
    }
}