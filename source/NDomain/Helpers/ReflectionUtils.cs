using NDomain.CQRS;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.Helpers
{
    internal static class ReflectionUtils
    {
        public static Func<string, TState, TAggregate> BuildCreateAggregateFromStateFunc<TAggregate, TState>()
            where TAggregate : IAggregate
            where TState : IState
        {
            var idParam = Expression.Parameter(typeof(string), "id");
            var stateParam = Expression.Parameter(typeof(TState), "state");

            var ctor = typeof(TAggregate).GetConstructor(new Type[] { typeof(string), typeof(TState) });

            var body = Expression.New(ctor, idParam, stateParam);
            var lambda = Expression.Lambda<Func<string, TState, TAggregate>>(body, idParam, stateParam);

            return lambda.Compile();
        }

        public static IEnumerable<Type> FindEventTypes<TAggregate>()
            where TAggregate : IAggregate
        {
            return FindEventTypes(typeof(TAggregate));
        }

        public static IEnumerable<Type> FindEventTypes(Type aggregateType)
        {
            var stateType = aggregateType.BaseType.GetGenericArguments()[0];

            var eventTypes = stateType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                                      .Where(m => m.Name.Length > 2 && m.Name.StartsWith("On") && m.GetParameters().Length == 1 && m.ReturnType == typeof(void))
                                      .Select(m => m.GetParameters()[0].ParameterType)
                                      .ToArray();

            return eventTypes;
        }

        public static Dictionary<string, Action<TState, object>> GetStateEventMutators<TState>()
            where TState : IState
        {
            var stateType = typeof(TState);

            var mutateMethods = stateType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                                         .Where(m => m.Name.Length > 2 && m.Name.StartsWith("On") && m.GetParameters().Length == 1 && m.ReturnType == typeof(void))
                                         .ToArray();

            var stateEventMutators = from method in mutateMethods
                                     let eventType = method.GetParameters()[0].ParameterType
                                     select new
                                     {
                                         Name = eventType.Name,
                                         Handler = BuildStateEventMutatorHandler<TState>(eventType, method)
                                     };

            return stateEventMutators.ToDictionary(m => m.Name, m => m.Handler);
        }

        private static Action<TState, object> BuildStateEventMutatorHandler<TState>(Type eventType, MethodInfo method)
            where TState : IState
        {
            var stateParam = Expression.Parameter(typeof(TState), "state");
            var eventParam = Expression.Parameter(typeof(object), "ev");

            // state.On((TEvent)ev)
            var methodCallExpr = Expression.Call(stateParam, 
                                                 method, 
                                                 Expression.Convert(eventParam, eventType));

            var lambda = Expression.Lambda<Action<TState, object>>(methodCallExpr, stateParam, eventParam);
            return lambda.Compile();
        }

        public static Dictionary<Type, Func<THandler, ICommand, Task>> FindCommandHandlerMethods<THandler>()
        {
            var methods = typeof(THandler)
                                  .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                                  .Where(m => m.Name == "Handle"
                                           && m.GetParameters().Length == 1 && typeof(ICommand).IsAssignableFrom(m.GetParameters()[0].ParameterType)
                                           && m.GetParameters()[0].ParameterType.IsGenericType
                                           && m.ReturnType == typeof(Task))
                                  .ToArray();

            var handlers = methods.ToDictionary(
                                    m => m.GetParameters()[0].ParameterType.GetGenericArguments()[0],
                                    m => BuildCommandHandler<THandler>(m));
            return handlers;
        }

        public static Dictionary<Type, Func<THandler, IEvent, Task>> FindEventHandlerMethods<THandler>()
        {
            var methods = typeof(THandler)
                                  .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                                  .Where(m => m.Name == "On"
                                           && m.GetParameters().Length == 1 && typeof(IEvent).IsAssignableFrom(m.GetParameters()[0].ParameterType)
                                           && m.GetParameters()[0].ParameterType.IsGenericType
                                           && m.ReturnType == typeof(Task))
                                  .ToArray();

            var handlers = methods.ToDictionary(
                                    m => m.GetParameters()[0].ParameterType.GetGenericArguments()[0],
                                    m => BuildEventHandler<THandler>(m));
            return handlers;
        }

        public static Dictionary<Type, Func<THandler, IAggregateEvent, Task>> FindAggregateEventHandlerMethods<THandler>()
        {
            var methods = typeof(THandler)
                                  .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                                  .Where(m => m.Name == "On"
                                           && m.GetParameters().Length == 1 && typeof(IAggregateEvent).IsAssignableFrom(m.GetParameters()[0].ParameterType)
                                           && m.GetParameters()[0].ParameterType.IsGenericType
                                           && m.ReturnType == typeof(Task))
                                  .ToArray();

            var handlers = methods.ToDictionary(
                                    m => m.GetParameters()[0].ParameterType.GetGenericArguments()[0],
                                    m => BuildAggregateEventHandler<THandler>(m));
            return handlers;
        }

        public static Dictionary<string, Action<T, IAggregateEvent>> FindQueryEventHandlerMethods<T>(object instance)
        {
            var methods = instance.GetType()
                                  .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                                  .Where(m => m.Name == "On"
                                           && m.GetParameters().Length == 2 && m.GetParameters()[0].ParameterType == typeof(T)
                                           && m.ReturnType == typeof(void))
                                  .ToArray();

            // TODO: EventName is hardcoded as Type.Name
            var handlers = methods.ToDictionary(m => m.GetParameters()[1].ParameterType.Name, m => BuildQueryEventHandler<T>(instance, m));
            return handlers;
        }

        private static Action<T, IAggregateEvent> BuildQueryEventHandler<T>(object instance, MethodInfo method)
        {
            var eventType = method.GetParameters()[1].ParameterType;

            var queryParam = Expression.Parameter(typeof(T), "query");
            var eventParam = Expression.Parameter(typeof(IAggregateEvent), "ev");

            // instance.On(query, ev.Payload)
            var methodCallExpr = Expression.Call(
                                    Expression.Constant(instance, instance.GetType()),
                                    method,
                                    queryParam,
                                    Expression.Convert(Expression.Property(eventParam, "Payload"), eventType));

            var lambda = Expression.Lambda<Action<T, IAggregateEvent>>(methodCallExpr, queryParam, eventParam);
            return lambda.Compile();
        }

        private static Func<THandler, ICommand, Task> BuildCommandHandler<THandler>(MethodInfo method)
        {
            var instanceParam = Expression.Parameter(typeof(THandler), "instance");

            var commandType = method.GetParameters()[0].ParameterType;

            var commandParam = Expression.Parameter(typeof(ICommand), "cmd");

            // instance.Handle(cmd as ICommand<T>)
            var methodCallExpr = Expression.Call(
                                    instanceParam,
                                    method,
                                    Expression.Convert(commandParam, commandType));

            var lambda = Expression.Lambda<Func<THandler, ICommand, Task>>(methodCallExpr, instanceParam, commandParam);
            return lambda.Compile();
        }

        private static Func<THandler, IEvent, Task> BuildEventHandler<THandler>(MethodInfo method)
        {
            var instanceParam = Expression.Parameter(typeof(THandler), "instance");

            var eventType = method.GetParameters()[0].ParameterType;

            var eventParam = Expression.Parameter(typeof(IEvent), "ev");

            // instance.On(ev as IEvent<T>)
            var methodCallExpr = Expression.Call(
                                    instanceParam,
                                    method,
                                    Expression.Convert(eventParam, eventType));

            var lambda = Expression.Lambda<Func<THandler, IEvent, Task>>(methodCallExpr, instanceParam, eventParam);
            return lambda.Compile();
        }

        private static Func<THandler, IAggregateEvent, Task> BuildAggregateEventHandler<THandler>(MethodInfo method)
        {
            var instanceParam = Expression.Parameter(typeof(THandler), "instance");

            var eventType = method.GetParameters()[0].ParameterType;

            var eventParam = Expression.Parameter(typeof(IAggregateEvent), "ev");

            // instance.On(ev as IAggregateEvent<T>)
            var methodCallExpr = Expression.Call(
                                    instanceParam,
                                    method,
                                    Expression.Convert(eventParam, eventType));

            var lambda = Expression.Lambda<Func<THandler, IAggregateEvent, Task>>(methodCallExpr, instanceParam, eventParam);
            return lambda.Compile();
        }
    }
}
