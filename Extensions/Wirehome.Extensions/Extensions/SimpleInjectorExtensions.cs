using Wirehome.Contracts.Core;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace Wirehome.Extensions.Extensions
{
    public static class SimpleInjectorExtensions
    {
        public static void RegisterServicesInNamespace(this IContainer containerService, IEnumerable<Assembly> assemblies, string namespaceName)
        {
            var registrations = assemblies.SelectMany(x => x.GetExportedTypes())
                                          .Where
                                          (
                                             x => x.Namespace == namespaceName
                                             && x.GetInterfaces().Any()
                                             && x.GetTypeInfo().IsClass
                                          )
                                          .Select(y => new
                                          {
                                            Service = y.GetInterfaces(false).Single(),
                                            Implementation = y
                                          }
                                          );

            foreach (var reg in registrations)
            {
                containerService.RegisterSingleton(reg.Service, reg.Implementation);
            }
        }
    }
}
