using NDomain.Logging;
using System;

namespace NDomain.Configuration
{
    /// <summary>
    /// Configurator for the logging capabilities
    /// </summary>
    public class LoggingConfigurator : Configurator
    {
        public LoggingConfigurator(ContextBuilder builder)
            : base(builder)
        {
            builder.Configuring += this.OnConfiguring;
        }

        /// <summary>
        /// Gets or sets the logger factory
        /// </summary>
        public ILoggerFactory LoggerFactory { get; set; }

        private void OnConfiguring(ContextBuilder builder)
        {
            builder.LoggerFactory = new Lazy<ILoggerFactory>(
                () => this.LoggerFactory ?? NullLoggerFactory.Instance);
        }
    }
}
