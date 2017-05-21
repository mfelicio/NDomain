using System;

namespace NDomain.Logging
{
    internal class NullLogger : ILogger
    {
        public static readonly NullLogger Instance = new NullLogger();

        private NullLogger() { }

        public void Debug(string message, params object[] args)
        {
            
        }

        public void Info(string message, params object[] args)
        {
            
        }

        public void Warn(string message, params object[] args)
        {
            
        }

        public void Warn(Exception exception, string message, params object[] args)
        {
            
        }

        public void Error(string message, params object[] args)
        {
            
        }

        public void Error(Exception exception, string message, params object[] args)
        {
            
        }

        public void Fatal(string message, params object[] args)
        {
            
        }

        public void Fatal(Exception exception, string message, params object[] args)
        {
            
        }
    }
}