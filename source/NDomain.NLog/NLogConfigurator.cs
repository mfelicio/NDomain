using NDomain.Configuration;
using NDomain.NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.Configuration
{
    /// <summary>
    /// Logging configurator that uses NLog
    /// </summary>
    public static class NLogConfigurator
    {
        /// <summary>
        /// Configures NDomain framework with NLog.
        /// </summary>
        /// <param name="configurator">logging configurator</param>
        /// <returns>Current configurator instance, to be used in a fluent manner</returns>
        public static LoggingConfigurator WithNLog(this LoggingConfigurator configurator)
        {
            configurator.LoggerFactory = new LoggerFactory();

            return configurator;
        }
    }
}
