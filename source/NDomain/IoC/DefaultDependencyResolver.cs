using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.IoC
{
    /// <summary>
    /// Simple IoC container that is used by default when no other IoC container was registered when building the DomainContext. This will only resolve internal NDomain components and may not work if your message handlers have other dependencies.
    /// </summary>
    public class DefaultDependencyResolver : IDependencyScope
    {
        readonly Dictionary<Type, object> knownInstances;
        readonly Dictionary<Type, Type> knownGenericTypeDefs;

        public DefaultDependencyResolver()
        {
            this.knownInstances = new Dictionary<Type, object>();
            this.knownGenericTypeDefs = new Dictionary<Type, Type>();
        }

        public DefaultDependencyResolver Register<T>(T instance)
        {
            this.knownInstances.Add(typeof(T), instance);
            return this;
        }

        public DefaultDependencyResolver RegisterInstances(Dictionary<Type, object> instances)
        {
            foreach (var registration in instances)
            {
                this.knownInstances[registration.Key] = registration.Value;
            }

            return this;
        }

        /// <summary>
        /// Intended to register typeof(IAggregateRepository<>), typeof(AggregateRepository<>)
        /// </summary>
        /// <param name="mapFrom"></param>
        /// <param name="mapTo"></param>
        /// <returns></returns>
        public DefaultDependencyResolver RegisterGenericTypeDef(Type mapFrom, Type mapTo)
        {
            if (!mapFrom.IsGenericTypeDefinition)
            {
                throw new ArgumentException("Should be generic type definitions", "mapFrom");
            }

            if (!mapTo.IsGenericTypeDefinition)
            {
                throw new ArgumentException("Should be generic type definitions", "mapTo");
            }

            this.knownGenericTypeDefs.Add(mapFrom, mapTo);
            return this;
        }

        public IDependencyScope BeginScope()
        {
            return this;
        }

        public IDependencyScope BeginScope(object tag)
        {
            return this;
        }

        public object Resolve(Type serviceType)
        {
            if (this.knownInstances.ContainsKey(serviceType))
            {
                return this.knownInstances[serviceType];
            }

            if(serviceType.IsInterface)
            {
                return ResolveService(serviceType, serviceType);
            }

            var ctors = serviceType.GetConstructors();
            if (ctors.Length != 1)
            {
                throw new Exception("DefaultDependencyResolver can only resolve services with one constructor");
            }

            var ctor = ctors[0];

            var args = ctor.GetParameters()
                           .Select(p => Resolve(p.ParameterType))
                           .ToArray();

            return Activator.CreateInstance(serviceType, args);
        }

        private object ResolveService(Type dependency, Type service)
        {
            if (dependency.IsGenericType)
            {
                Type mapTo;
                if (this.knownGenericTypeDefs.TryGetValue(dependency.GetGenericTypeDefinition(), out mapTo))
                {
                    return ResolveParameter(dependency, mapTo);
                }
            }

            throw new Exception(string.Format("DefaultDependencyResolver cannot resolve dependency {0} when activating type {1}", dependency, service));
        }

        private object ResolveParameter(Type mapFrom, Type mapTo)
        {
            var genericArg = mapFrom.GetGenericArguments()[0];

            var resolveType = mapTo.MakeGenericType(genericArg);
            return Resolve(resolveType);
        }

        public T Resolve<T>()
        {
            return (T)Resolve(typeof(T));
        }

        public void Dispose() { }
    }
}
