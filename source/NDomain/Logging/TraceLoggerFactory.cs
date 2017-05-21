using System;

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
