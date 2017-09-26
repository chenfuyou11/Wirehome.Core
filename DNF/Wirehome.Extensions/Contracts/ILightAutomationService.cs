using System.Collections.Generic;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Areas;
using System;

namespace HA4IoT.Extensions.Contracts
{
    public interface ILightAutomationService
    {
        void MonitorArea(IArea area, TimeSpan timeOn);
    }
}