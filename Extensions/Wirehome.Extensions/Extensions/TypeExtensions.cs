using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Wirehome.Extensions.Extensions
{
    public static class TypeExtensions
    {
        public static IEnumerable<Type> GetInterfaces(this Type type, bool includeInherited)
        {
            var types = type.GetInterfaces();
            if (includeInherited) return types;
            
            return types.Except(types.SelectMany(t => t.GetInterfaces()));
        }
    }
}
