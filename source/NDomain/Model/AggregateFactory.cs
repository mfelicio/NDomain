using NDomain.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NDomain
{
    /// <summary>
    /// Helper class used to create generic IAggregateFactory instances.
    /// This is used mostly within the framework, but can be used outside as well.
    /// </summary>
    public static class AggregateFactory
    {
        /// <summary>
        /// Cached instances
        /// </summary>
        static readonly ConcurrentDictionary<Type, object> factories;

        static AggregateFactory()
        {
            factories = new ConcurrentDictionary<Type, object>();
        }

        /// <summary>
        /// Gets an IAggregateFactory for <typeparamref name="TAggregate"/>.
        /// Since this is a heavy operation, it is advised to cache the returned instance.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <typeparam name="TAggregate"></typeparam>
        /// <returns></returns>
        public static IAggregateFactory<TAggregate> For<TAggregate>()
            where TAggregate : IAggregate
        {
            var factory = factories.GetOrAdd(typeof(TAggregate), t => CreateFactory<TAggregate>());

            return factory as IAggregateFactory<TAggregate>;
        }

        private static IAggregateFactory<TAggregate> CreateFactory<TAggregate>()
            where TAggregate : IAggregate
        {
            var stateType = typeof(TAggregate).BaseType.GetGenericArguments()[0];

            var factoryType = typeof(AggregateFactory<,>).MakeGenericType(typeof(TAggregate), stateType);

            return Activator.CreateInstance(factoryType) as IAggregateFactory<TAggregate>;
        }
    }

    internal class AggregateFactory<TAggregate, TState> : IAggregateFactory<TAggregate>
        where TAggregate : IAggregate<TState>
        where TState : IState, new()
    {
        static readonly Func<string, TState, TAggregate> createAggregate;

        static AggregateFactory()
        {
            // runtime generated function that calls the aggregate's constructor passing the state as parameter
            createAggregate = ReflectionUtils.BuildCreateAggregateFromStateFunc<TAggregate, TState>();
        }

        public TAggregate CreateNew(string id)
        {
            var state = new TState();

            return createAggregate(id, state);
        }

        public TAggregate CreateFromEvents(string id, IAggregateEvent[] events)
        {
            var state = new TState();
            
            // rebuild the state from past events
            foreach (var @event in events)
            {
                state.Mutate(@event);
            }

            return createAggregate(id, state);
        }

        public TAggregate CreateFromState(string id, IState state)
        {
            return createAggregate(id, (TState)state);
        }
    }
}
