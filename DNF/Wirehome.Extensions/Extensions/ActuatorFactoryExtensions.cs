using System;
using Wirehome.Actuators;
using Wirehome.Contracts.Actuators;
using Wirehome.Contracts.Areas;
using Wirehome.Extensions.Core;
using Wirehome.Extensions.Contracts;

namespace Wirehome.Extensions.Extensions
{
    public static class ActuatorFactoryExtensions
    {
        public static ILamp RegisterMonostableLamp(this ActuatorFactory factory, IArea area, Enum id, IMonostableLampAdapter adapter)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));
            if (adapter == null) throw new ArgumentNullException(nameof(adapter));

            var lamp = new MonostableLamp($"{area.Id}.{id}", adapter);
            area.RegisterComponent(lamp);

            return lamp;
        }
    }
}
