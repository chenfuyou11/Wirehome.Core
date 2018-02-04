using System;
using System.Collections.Generic;
using System.Linq;

namespace Wirehome.Extensions.Extensions
{
    public static class TypeExtensions
    {
        public static IEnumerable<Type> GetDirectInterfaces(this Type type)
        {
            var actualInterfaces = type.GetInterfaces();
            foreach (var result in actualInterfaces
                .Except(type.BaseType?.GetInterfaces() ?? Enumerable.Empty<Type>()) //See https://stackoverflow.com/a/1613936
                .Except(actualInterfaces.SelectMany(i => i.GetInterfaces())) //See https://stackoverflow.com/a/5318781
            )
            {
                yield return result;
            }
        }
    }
}
