using System.Collections.Generic;
using Wirehome.Contracts.Components;
using Wirehome.Contracts.Services;
using Wirehome.Contracts.Areas;
using System;

namespace Wirehome.Extensions.Contracts
{
    public interface ILightAutomationService
    {
        void MonitorArea(IArea area, TimeSpan timeOn);
    }
}