using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Wirehome.Contracts.Core;

namespace Wirehome.Core
{
    public class Container : IContainer
    {
        private readonly SimpleInjector.Container _container = new SimpleInjector.Container();
        private readonly ControllerOptions _options;

        public Container(ControllerOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));

            _container.RegisterSingleton(options);
        }

        public void Verify()
        {
            _container.Verify();
        }

        public bool ChackNativeImpelentationExists()
        {
            var registrations =_container.GetCurrentRegistrations();
            if(!registrations.Any(x => x.ServiceType == typeof(INativeI2cBus)))
            {
                return false;
            }
            return true;
        }

        public IList<InstanceProducer> GetCurrentRegistrations()
        {
            return _container.GetCurrentRegistrations().ToList();
        }

        public TContract GetInstance<TContract>() where TContract : class
        {
            return _container.GetInstance<TContract>();
        }

        public object GetInstance(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            return _container.GetInstance(type);
        }
        
        public IList<TContract> GetInstances<TContract>() where TContract : class
        {
            var services = new List<TContract>();

            foreach (var registration in _container.GetCurrentRegistrations())
            {
                if (typeof(TContract).IsAssignableFrom(registration.ServiceType))
                {
                    services.Add((TContract)registration.GetInstance());
                }
            }

            return services;
        }

        public void RegisterFactory<T>(Func<T> factory) where T : class
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            _container.Register(factory);
        }

        public void RegisterSingleton<TImplementation>() where TImplementation : class
        {
            _container.RegisterSingleton<TImplementation>();
        }

        public void RegisterSingleton<TContract, TImplementation>() where TContract : class where TImplementation : class, TContract
        {
            _container.RegisterSingleton<TContract, TImplementation>();
        }

        public void RegisterSingleton(Type service, Type implementation) 
        {
            _container.Register(service, implementation, Lifestyle.Singleton);
        }

        public void RegisterSingleton<T>(T service) where T : class
        {
            _container.RegisterSingleton<T>(service);
        }

        public void RegisterCollection<TItem>(IEnumerable<TItem> items) where TItem : class
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            _container.RegisterCollection(items);
        }

        public void RegisterCollection<TItem>(IEnumerable<Assembly> assemblies) where TItem : class
        {
            if (assemblies == null) throw new ArgumentNullException(nameof(assemblies));

            _container.RegisterCollection(typeof(TItem), assemblies);
        }

        public void RegisterSingleton<TContract>(Func<TContract> instanceCreator) where TContract : class
        {
            if (instanceCreator == null) throw new ArgumentNullException(nameof(instanceCreator));

            _container.RegisterSingleton(instanceCreator);
        }

        public void RegisterType<T>() where T : class
        {
            _container.Register<T>();
        }

        public void RegisterInitializer<T>(Action<T> initializer) where T : class
        {
            _container.RegisterInitializer<T>(initializer);
        }
        




    }
}
