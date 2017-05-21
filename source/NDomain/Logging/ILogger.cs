using System;

namespace NDomain.Logging
{
    /// <summary>
    /// ILogger abstraction used when logging messages from the NDomain framework components
    /// </summary>
    public interface ILogger
    {
        void Debug(string message, params object[] args);

        void Info(string message, params object[] args);
        
        void Warn(string message, params object[] args);
        void Warn(Exception exception, string message, params object[] args);
        
        void Error(string message, params object[] args);
        void Error(Exception exception, string message, params object[] args);

        void Fatal(string message, params object[] args);
        void Fatal(Exception exception, string message, params object[] args);
    }
}
