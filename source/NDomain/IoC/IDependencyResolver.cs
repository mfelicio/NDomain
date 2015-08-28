using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.IoC
{
    public interface IDependencyResolver : IDisposable
    {
        IDependencyScope BeginScope();
        IDependencyScope BeginScope(object tag);

        object Resolve(Type serviceType);
        T Resolve<T>();
    }

    public interface IDependencyScope : IDependencyResolver
    {
        
    }
}
