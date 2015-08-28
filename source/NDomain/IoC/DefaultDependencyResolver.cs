using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.IoC
{
    public class DefaultDependencyResolver : IDependencyScope
    {
        readonly Dictionary<Type, object> knownInstances;
        readonly Dictionary<Type, Type> knownGenericTypeDefs;

        public DefaultDependencyResolver()
        {
            this.knownInstances = new Dictionary<Type, object>();
            this.knownGenericTypeDefs = new Dictionary<Type, Type>();
            this.knownInstances.Add(typeof(IDependencyResolver), this);
        }

        public DefaultDependencyResolver Register<T>(T instance)
        {
            this.knownInstances.Add(typeof(T), instance);
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
            var ctors = serviceType.GetConstructors();
            if (ctors.Length != 1)
            {
                throw new Exception("DefaultDependencyResolver can only resolve services with one constructor");
            }

            var ctor = ctors[0];

            var args = ctor.GetParameters()
                           .Select(p => {
                               object arg;
                               if (this.knownInstances.TryGetValue(p.ParameterType, out arg))
                               {
                                   return arg;
                               }

                               Type mapTo;
                               if (p.ParameterType.IsGenericType && this.knownGenericTypeDefs.TryGetValue(p.ParameterType.GetGenericTypeDefinition(), out mapTo))
                               {
                                   return ResolveParameter(p.ParameterType, mapTo);
                               }

                               throw new Exception(string.Format("DefaultDependencyResolver cannot resolve parameter {0} when activating type {1}", p.ParameterType, serviceType));
                           });

            return Activator.CreateInstance(serviceType, args.ToArray());
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
