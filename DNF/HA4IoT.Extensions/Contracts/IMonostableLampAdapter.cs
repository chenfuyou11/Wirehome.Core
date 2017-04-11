using System;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Adapters;
using HA4IoT.Contracts.Components.States;

namespace HA4IoT.Extensions.Core
{
    public interface IMonostableLampAdapter : ILampAdapter
    {
        event Action<PowerStateValue> StateChanged;
    }
}