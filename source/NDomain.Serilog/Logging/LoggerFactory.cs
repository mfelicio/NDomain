using System;
using NDomain.Logging;

namespace NDomain.Serilog.Logging
{
	public class LoggerFactory : ILoggerFactory
	{
		public ILogger GetLogger(string name)
		{
			return new Logger(global::Serilog.Log.Logger.ForContext("SourceContext", name));
		}

		public ILogger GetLogger(Type type)
		{
			return new Logger(global::Serilog.Log.Logger.ForContext(type));
		}
	}
}