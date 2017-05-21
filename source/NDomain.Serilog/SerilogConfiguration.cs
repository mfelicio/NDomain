using NDomain.Serilog.Logging;

// ReSharper disable once CheckNamespace
namespace NDomain.Configuration
{
	public static class SerilogConfigurator
	{
		public static LoggingConfigurator WithSerilog(this LoggingConfigurator configurator)
		{
			configurator.LoggerFactory = new LoggerFactory();

			return configurator;
		}
	}
}