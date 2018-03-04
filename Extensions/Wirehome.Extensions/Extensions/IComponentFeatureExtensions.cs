using Wirehome.Contracts.Components;
using Wirehome.Contracts.Components.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WWirehome.Core.Utils;

namespace Wirehome.Extensions.Extensions
{
    public static class IComponentFeatureExtensions
    {
        public static IEnumerable<Type> SupportedCommands(this IComponentFeature feature)
        {
            var assemblies = AssemblyHelper.GetProjectAssemblies();
            var commands = new List<Type>();
            foreach(var type in GetTypesWithAttribute<FeatureAttribute>(assemblies, feature.GetType()))
            {
                commands.AddRange(GetTypesWithAttribute<FeatureStateAttribute>(assemblies, type));
            }
            return commands;
        }

        private static IEnumerable<Type> GetTypesWithAttribute<T>(IEnumerable<Assembly> assemblies, Type componentFeatureType) where T: ITypeAssociation
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
