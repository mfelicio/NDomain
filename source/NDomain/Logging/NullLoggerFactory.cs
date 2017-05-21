using System;

namespace NDomain.Logging
{
    /// <summary>
    /// ILoggerFactory implementation to be used when no logging is intended.
    /// Besides unit tests, this should never be used.
    /// </summary>
    public class NullLoggerFactory : ILoggerFactory
    {
        /// <summary>
        /// Singleton instance for the NullLoggerFactory
        /// </summary>
        public static readonly ILoggerFactory Instance = new NullLoggerFactory();

        private NullLoggerFactory() { }

        public ILogger GetLogger(string name)
        {
            return NullLogger.Instance;
        }

        public ILogger GetLogger(Type type)
        {
            return NullLogger.Instance;
        }
    }
}
