using NDomain.Logging;
using System;

namespace NDomain.Serilog
{
	public class Logger : ILogger
	{
		global::Serilog.ILogger _logger;

		public Logger(global::Serilog.ILogger logger)
		{
			_logger = logger;
		}

		public void Debug(string message, params object[] args)
		{
			_logger.Debug(message, args);
		}

		public void Info(string message, params object[] args)
		{
			_logger.Information(message, args);
		}

		public void Warn(string message, params object[] args)
		{
			_logger.Warning(message, args);
		}

		public void Warn(Exception exception, string message, params object[] args)
		{
			_logger.Warning(exception, string.Format(message, args));
		}

		public void Error(string message, params object[] args)
		{
			_logger.Error(message, args);
		}

		public void Error(Exception exception, string message, params object[] args)
		{
			_logger.Error(exception, message, args);
		}

		public void Fatal(string message, params object[] args)
		{
			_logger.Fatal(message, args);
		}

		public void Fatal(Exception exception, string message, params object[] args)
		{
			_logger.Fatal(exception, string.Format(message, args));
		}
	}
}