using System;
using NDomain.Logging;

namespace NDomain.NLog.Logging
{
    /// <summary>
    /// ILoggerFactory based on NLog
    /// </summary>
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
