using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    /// <summary>
    /// IDependencyScope abstraction used when resolving message handlers, so that each message is processed within its own dependency scope.
    /// </summary>
    public interface IDependencyScope : IDependencyResolver
    {
        
    }
}
