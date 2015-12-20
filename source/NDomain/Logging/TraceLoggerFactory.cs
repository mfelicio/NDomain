using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.Logging
{
    /// <summary>
    /// ILoggerFactory implementation that returns ILogger implementations that use the Trace.
    /// </summary>
    public class TraceLoggerFactory : ILoggerFactory
    {

        public ILogger GetLogger(string name)
        {
            return new TraceLogger(name);
        }

        public ILogger GetLogger(Type type)
        {
            return new TraceLogger(type.FullName);
        }
    }
}
