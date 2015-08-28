using NDomain.Configuration;
using NDomain.NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.Configuration
{
    public static class NLogConfigurator
    {
        public static LoggingConfigurator WithNLog(this LoggingConfigurator configurator)
        {
            configurator.LoggerFactory = new LoggerFactory();

            return configurator;
        }
    }
}
