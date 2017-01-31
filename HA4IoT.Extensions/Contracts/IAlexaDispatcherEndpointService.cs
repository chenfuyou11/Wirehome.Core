using System.Collections.Generic;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Extensions
{
    public interface IAlexaDispatcherEndpointService
    {
        void AddConnectedVivices(string friendlyName, IEnumerable<IComponent> devices);
    }
}