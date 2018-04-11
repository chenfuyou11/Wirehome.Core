using Wirehome.Contracts.Services;
using System.Collections.Generic;

namespace Wirehome.Contracts.Areas
{
    public interface IAreaRegistryService : IService
    {
        IArea RegisterArea(string id);
        
        IArea GetArea(string id);

        IList<IArea> GetAreas();
    }
}
