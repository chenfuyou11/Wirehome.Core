using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Wirehome.Contracts.Core
{
    public interface IContainer
    {
        TContract GetInstance<TContract>() where TContract : class;

        object GetInstance(Type type);

        IList<TContract> GetInstances<TContract>() where TContract : class;
        
        void RegisterSingleton<TImplementation>() where TImplementation : class;

        void RegisterSingleton<TImplementation>(Func<TImplementation> instanceCreator) where TImplementation : class;

        void RegisterSingleton<TContract, TImplementation>() where TContract : class
            where TImplementation : class, TContract;

        void RegisterCollection<TItem>(IEnumerable<TItem> items) where TItem : class;

        void RegisterCollection<TItem>(IEnumerable<Assembly> assemblies) where TItem : class;

        void RegisterSingleton(Type service, Type implementation);

        void RegisterFactory<T>(Func<T> factory) where T : class;

        void RegisterType<T>() where T : class;

        void RegisterInitializer<T>(Action<T> initializer) where T : class;

        void RegisterSingleton<T>(T service) where T : class;
    }
}