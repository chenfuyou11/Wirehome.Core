using System.Collections.Generic;
using Wirehome.Contracts.Components;
using Wirehome.Contracts.Services;

namespace Wirehome.Extensions.Contracts
{
    public interface IAlexaDispatcherEndpointService : IService
    {
        void AddConnectedVivices(string friendlyName, IEnumerable<IComponent> devices);
    }
}