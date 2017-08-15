using HA4IoT.Contracts.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace HA4IoT.Extensions.Extensions
{
    public static class SimpleInjectorExtensions
    {
        public static void RegisterServicesInNamespace(this IContainer containerService, Assembly repositoryAssembly, string namespaceName)
        {

            var registrations = repositoryAssembly.GetExportedTypes()
                                                  .Where
                                                  ( 
                                                    x => x.Namespace == namespaceName &&
                                                    x.GetInterfaces().Any() &&
                                                    x.GetTypeInfo().IsClass
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
