using NDomain.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.NLog
{
    public class LoggerFactory : ILoggerFactory
    {
        public ILogger GetLogger(string name)
        {
            return new Logger(global::NLog.LogManager.GetLogger(name));
        }

        public ILogger GetLogger(Type type)
        {
            return new Logger(global::NLog.LogManager.GetLogger(type.FullName));
        }
    }
}
