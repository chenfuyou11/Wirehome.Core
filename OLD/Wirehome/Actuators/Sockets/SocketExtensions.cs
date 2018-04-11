using System;
using Wirehome.Contracts.Actuators;
using Wirehome.Contracts.Areas;

namespace Wirehome.Actuators.Sockets
{
    public static class SocketExtensions
    {
        public static ISocket GetSocket(this IArea area, Enum id)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            return area.GetComponent<ISocket>($"{area.Id}.{id}");
        }
    }
}
