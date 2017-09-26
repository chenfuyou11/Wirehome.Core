using System;
using System.Reflection;
using Wirehome.Contracts.Api;

namespace Wirehome.Contracts.Services
{
    public class ApiServiceClassAttribute : ApiClassAttribute
    {
        public ApiServiceClassAttribute(Type interfaceType)
        {
            if (interfaceType == null) throw new ArgumentNullException(nameof(interfaceType));

            if (!typeof(IService).GetTypeInfo().IsAssignableFrom(interfaceType.GetTypeInfo()))
            {
                throw new ArgumentException($"Type {interfaceType.FullName} is not implementing IService.");
            }
            
            Namespace = "Service/" + interfaceType.Name;
        }
    }
}
