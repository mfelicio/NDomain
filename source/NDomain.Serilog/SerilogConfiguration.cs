using NDomain.Serilog;

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