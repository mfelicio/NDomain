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
    /// Internal cache for StateMutator instances, as their construction is very heavy
    /// </summary>
    internal static class StateMutator
    {
        static readonly ConcurrentDictionary<Type, IStateMutator> mutators;

        static StateMutator()
        {
            mutators = new ConcurrentDictionary<Type, IStateMutator>();
        }

        public static IStateMutator For(Type stateType)
        {
            return mutators.GetOrAdd(stateType, t => CreateMutator(stateType));
        }

        private static IStateMutator CreateMutator(Type stateType)
        {
            var mutatorType = typeof(StateMutator<>).MakeGenericType(stateType);
            var mutator = Activator.CreateInstance(mutatorType);

            return mutator as IStateMutator;
        }
    }

    /// <summary>
    /// Internal implementation for a StateMutator, based on a convention for methods that apply events.
    /// It will generate lambda expressions in runtime, compile and cache them, that invoke methods with the conventioned signature.

    /// The convention used is void On[NameOfTheEventType]([NameOfTheEventType] ev)
    /// </summary>
    /// <typeparam name="TState">Type of the state</typeparam>
    internal class StateMutator<TState> : IStateMutator
        where TState : IState
    {
        readonly Dictionary<string, Action<TState, object>> eventMutators;

        public StateMutator()
        {
            this.eventMutators = ReflectionUtils.GetStateEventMutators<TState>();
        }

        public void Mutate(IState state, IAggregateEvent @event)
        {
            var eventMutator = this.eventMutators[@event.Name];
            eventMutator((TState)state, @event.Payload);
        }
    }
}
