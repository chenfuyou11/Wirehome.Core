using Wirehome.Contracts.Components;
using Wirehome.Contracts.Components.Attributes;
using Wirehome.Contracts.Components.Commands;
using Wirehome.Contracts.Components.Features;
using Wirehome.Extensions.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Wirehome.Extensions.Extensions
{
    public static class IComponentFeatureExtensions
    {
        public static IEnumerable<Type> SupportedCommands(this IComponentFeature feature)
        {
            //TODO DNF - change it in .NET Standard 2.0 to all assemblies
            var assemblies = new[] { typeof(IComponentFeature).GetTypeInfo().Assembly, typeof(IAlexaDispatcherEndpointService).GetTypeInfo().Assembly };

            var commands = new List<Type>();
            foreach(var type in GetTypesWithAttribute<FeatureAttribute>(assemblies, feature.GetType()))
            {
                commands.AddRange(GetTypesWithAttribute<FeatureStateAttribute>(assemblies, type));
            }
            
            return commands;
        }

        static IEnumerable<Type> GetTypesWithAttribute<T>(Assembly[] assemblies, Type componentFeatureType) where T: ITypeAssociation
        {
            foreach (Assembly assembly in assemblies)
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if(type.GetTypeInfo().GetCustomAttributes(typeof(T), true).Cast<T>().Where(x => x.AssociateType == componentFeatureType).Any())
                    {
                        yield return type;
                    }
                }
            }
        }
    }
}
