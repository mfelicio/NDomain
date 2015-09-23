using NDomain.Logging;
using System;

namespace NDomain.Serilog
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