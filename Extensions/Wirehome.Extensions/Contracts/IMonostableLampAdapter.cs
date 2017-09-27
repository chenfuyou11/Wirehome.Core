using System;
using Wirehome.Contracts.Components.States;
using Wirehome.Contracts.Components.Adapters;

namespace Wirehome.Extensions.Contracts
{
    public interface IMonostableLampAdapter : ILampAdapter
    {
        event Action<PowerStateValue> StateChanged;
    }
}