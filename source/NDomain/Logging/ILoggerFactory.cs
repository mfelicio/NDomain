using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.Logging
{
    public interface ILoggerFactory
    {
        ILogger GetLogger(string name);
        ILogger GetLogger(Type type);
    }
}
