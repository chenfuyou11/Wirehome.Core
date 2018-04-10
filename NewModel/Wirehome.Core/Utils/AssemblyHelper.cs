using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Wirehome.Core.Extensions;

namespace Wirehome.Core.Utils
{
    public static class AssemblyHelper
    {
        private const string TestAssembliesName = "Tests";

        public static IEnumerable<Assembly> GetProjectAssemblies(bool ignoreTestAssemblies = true)
        {
            var mainAssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            var applicationNameName = mainAssemblyName.Substring(0, mainAssemblyName.IndexOf("."));
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            var list = assemblies.Where(x => x.FullName.IndexOf(applicationNameName) > -1);
            if (ignoreTestAssemblies)
            {
                list = list.Where(x => x.FullName.IndexOf(TestAssembliesName) == -1);
            }

            return list;
        }

        public static IEnumerable<Type> GetAllInherited<T>() => GetProjectAssemblies().SelectMany(s => s.GetTypes())
                                                                                      .Where(p => typeof(T).IsAssignableFrom(p) && !p.IsAbstract);

        public static IEnumerable<string> GetReferencedAssemblies(Type sourceType)
        {
            var allAsseblies = AppDomain.CurrentDomain.GetAssemblies();
            var referenced = sourceType.GetTypeInfo().Assembly.GetReferencedAssemblies().Select(a => a.FullName).ToList().AddChained(sourceType.GetTypeInfo().Assembly.FullName);

            return referenced.Select(assembly => allAsseblies.FirstOrDefault(a => a.FullName == assembly)?.Location).Where(x => !string.IsNullOrWhiteSpace(x));
        }
    }
}