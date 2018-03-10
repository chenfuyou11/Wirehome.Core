using Quartz;
using Quartz.Spi;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Wirehome.ComponentModel.Adapters;
using Wirehome.ComponentModel.Configuration;
using Wirehome.Core.Communication.I2C;
using Wirehome.Core.EventAggregator;
using Wirehome.Core.Native;
using Wirehome.Core.Services.Logging;
using Wirehome.Core.Services.Quartz;

namespace Wirehome.Core.Services.DependencyInjection
{
    public class WirehomeContainer : IContainer
    {
        private readonly Container _container = new Container();
        private readonly ControllerOptions _options;
        private Dictionary<Type, int> _serviceInitalizationPriority = new Dictionary<Type, int>();

        public WirehomeContainer(ControllerOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));

            _container.RegisterSingleton(options);
        }

         public Action<Container> RegisterBaseServices { get; set; } = container =>
         {
             container.RegisterSingleton<IEventAggregator, EventAggregator.EventAggregator>();
             container.RegisterSingleton<IConfigurationService, ConfigurationService>();
             container.RegisterSingleton<II2CBusService, I2CBusService>();
             container.RegisterSingleton<ILogService, LogService>();
             container.RegisterSingleton<IAdapterServiceFactory, AdapterServiceFactory>();

             //Quartz
             container.RegisterSingleton<IJobFactory, SimpleInjectorJobFactory>();
             container.RegisterSingleton<ISchedulerFactory, SimpleInjectorSchedulerFactory>();
             container.Register(() => container.GetInstance<ISchedulerFactory>().GetScheduler().Result);

             //Auto mapper
             container.RegisterSingleton(() => container.GetInstance<MapperProvider>().GetMapper());
         };

        public IContainer RegisterServices()
        {
            _container.RegisterSingleton<IContainer>(this);
            RegisterBaseServices(_container);
            return this;
        }

        public void Verify()
        {
            _container.Verify();
        }

        public bool ChackNativeImpelentationExists()
        {
            var registrations = _container.GetCurrentRegistrations();
            if (!registrations.Any(x => x.ServiceType == typeof(INativeI2cBus)))
            {
                return false;
            }
            return true;
        }

        public IList<InstanceProducer> GetCurrentRegistrations()
        {
            return _container.GetCurrentRegistrations().ToList();
        }

        public IEnumerable<InstanceProducer> GetSingletonRegistrations()
        {
            return _container.GetCurrentRegistrations();
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


        public void RegisterService<TContract, TImplementation>(int priority = 0) where TContract : class, IService where TImplementation : class, TContract
        {
            RegisterSingleton<TContract, TImplementation>();
            _serviceInitalizationPriority.Add(typeof(TImplementation), priority);
        }

        public void RegisterSingleton(Type service, object instance)
        {
            _container.RegisterSingleton(service, instance);
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

        public Queue<IService> GetSerives()
        {
            var services = GetInstances<IService>().ToList();
            var result = new Queue<IService>();

            // add priority at first
            foreach (var service in _serviceInitalizationPriority.Where(v => v.Value > 0).OrderByDescending(x => x.Value).Select(y => y.Key))
            {
                var found = services.Find(s => s.GetType() == service);
                result.Enqueue(found);
                services.Remove(found);
            }

            foreach (var service in services)
            {
                result.Enqueue(service);
            }

            return result;
        }

    }
}