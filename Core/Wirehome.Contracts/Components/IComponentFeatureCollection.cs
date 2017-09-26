using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Wirehome.Contracts.Components
{
    public interface IComponentFeatureCollection
    {
        TComponentFeature Extract<TComponentFeature>() where TComponentFeature : IComponentFeature;
        bool Has(IComponentFeature state);
        bool Supports<TComponentFeature>() where TComponentFeature : IComponentFeature;
        IComponentFeatureCollection With(IComponentFeature feature);

        Dictionary<string, JToken> Serialize();
    }
}