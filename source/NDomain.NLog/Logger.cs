using NDomain.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.NLog
{
    /// <summary>
    /// NLog logger adapter
    /// </summary>
    public class Logger : ILogger
    {
        global::NLog.Logger logger;

        public Logger(global::NLog.Logger logger)
        {
            this.logger = logger;
        }

        public void Debug(string message, params object[] args)
        {
            this.logger.Debug(message, args);
        }

        public void Info(string message, params object[] args)
        {
            this.logger.Info(message, args);
        }

        public void Warn(string message, params object[] args)
        {
            this.logger.Warn(message, args);
        }

        public void Warn(Exception exception, string message, params object[] args)
        {
            this.logger.Warn(string.Format(message, args), exception);
        }

        public void Error(string message, params object[] args)
        {
            this.logger.Error(message, args);
        }

        public void Error(Exception exception, string message, params object[] args)
        {
            this.logger.Error(string.Format(message, args), exception);
        }

        public void Fatal(string message, params object[] args)
        {
            this.logger.Fatal(message, args);
        }

        public void Fatal(Exception exception, string message, params object[] args)
        {
            this.logger.Fatal(string.Format(message, args), exception);
        }
    }
}
