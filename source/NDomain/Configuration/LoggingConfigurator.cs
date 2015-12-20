using NDomain.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.Configuration
{
    /// <summary>
    /// Configurator for the logging capabilities
    /// </summary>
    public class LoggingConfigurator : Configurator
    {
        /// <summary>
        /// Gets or sets the logger factory
        /// </summary>
        public ILoggerFactory LoggerFactory { get; set; }

        public LoggingConfigurator(ContextBuilder builder)
            : base(builder)
        {
            builder.Configuring += this.OnConfiguring;
        }

        private void OnConfiguring(ContextBuilder builder)
        {
            builder.LoggerFactory = new Lazy<ILoggerFactory>(
                () => this.LoggerFactory ?? NullLoggerFactory.Instance);
        }
    }
}
