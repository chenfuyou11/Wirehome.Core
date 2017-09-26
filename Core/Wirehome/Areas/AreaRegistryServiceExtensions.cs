using System;
using Wirehome.Contracts.Areas;

namespace Wirehome.Areas
{
    public static class AreaRegistryServiceExtensions
    {
        public static IArea RegisterArea(this IAreaRegistryService areaService, Enum id)
        {
            if (areaService == null) throw new ArgumentNullException(nameof(areaService));
            return areaService.RegisterArea(id.ToString());
        }

        public static IArea GetArea(this IAreaRegistryService areaService, Enum id)
        {
            if (areaService == null) throw new ArgumentNullException(nameof(areaService));

            return areaService.GetArea(id.ToString());
        }
    }
}
