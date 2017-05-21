using System;

namespace NDomain.IoC
{
    /// <summary>
    /// IDependencyResolver abstraction used to resolve message handlers
    /// </summary>
    public interface IDependencyResolver : IDisposable
    {
        IDependencyScope BeginScope();
        IDependencyScope BeginScope(object tag);

        object Resolve(Type serviceType);
        T Resolve<T>();
    }
}
