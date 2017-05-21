using System;
using NDomain.Logging;

namespace NDomain.Serilog.Logging
{
	public class Logger : ILogger
	{
		private global::Serilog.ILogger logger;

		public Logger(global::Serilog.ILogger logger)
		{
			this.logger = logger;
		}

		public void Debug(string message, params object[] args)
		{
			this.logger.Debug(message, args);
		}

		public void Info(string message, params object[] args)
		{
			this.logger.Information(message, args);
		}

		public void Warn(string message, params object[] args)
		{
			this.logger.Warning(message, args);
		}

		public void Warn(Exception exception, string message, params object[] args)
		{
			this.logger.Warning(exception, string.Format(message, args));
		}

		public void Error(string message, params object[] args)
		{
			this.logger.Error(message, args);
		}

		public void Error(Exception exception, string message, params object[] args)
		{
			this.logger.Error(exception, message, args);
		}

		public void Fatal(string message, params object[] args)
		{
			this.logger.Fatal(message, args);
		}

		public void Fatal(Exception exception, string message, params object[] args)
		{
			this.logger.Fatal(exception, string.Format(message, args));
		}
	}
}