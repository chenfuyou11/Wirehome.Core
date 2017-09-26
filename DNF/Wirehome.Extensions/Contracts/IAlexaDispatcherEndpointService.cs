using System.Collections.Generic;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Extensions.Contracts
{
    public interface IAlexaDispatcherEndpointService : IService
    {
        void AddConnectedVivices(string friendlyName, IEnumerable<IComponent> devices);
    }
}