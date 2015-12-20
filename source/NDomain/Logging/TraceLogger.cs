using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.Logging
{
    /// <summary>
    /// ILogger implementation that uses the Trace to write logs to all configured trace listeners
    /// </summary>
    public class TraceLogger : ILogger
    {
        readonly string name;

        public TraceLogger(string name)
        {
            this.name = name;
        }

        private string BuildMessage(string message, params object[] args)
        {
            return string.Format("{0} - {1}", name, string.Format(message, args));
        }

        private string BuildErrorMessage(Exception exception, string message, params object[] args)
        {
            var msg = BuildMessage(message, args);

            return string.Format("{0}\n{1}", msg, exception.ToString());
        }

        public void Debug(string message, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine(BuildMessage(message, args));
        }

        public void Info(string message, params object[] args)
        {
            Trace.TraceInformation(BuildMessage(message, args));
        }

        public void Warn(string message, params object[] args)
        {
            Trace.TraceWarning(BuildMessage(message, args));
        }

        public void Warn(Exception exception, string message, params object[] args)
        {
            Trace.TraceWarning(BuildErrorMessage(exception, message, args));
        }

        public void Error(string message, params object[] args)
        {
            Trace.TraceError(BuildMessage(message, args));
        }

        public void Error(Exception exception, string message, params object[] args)
        {
            Trace.TraceError(BuildErrorMessage(exception, message, args));
        }

        public void Fatal(string message, params object[] args)
        {
            Trace.TraceError(BuildMessage(message, args));
        }

        public void Fatal(Exception exception, string message, params object[] args)
        {
            Trace.TraceError(BuildErrorMessage(exception, message, args));
        }
    }
}
