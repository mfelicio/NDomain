using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.Logging
{
    /// <summary>
    /// ILoggerFactory abstraction to get specific logger instances
    /// </summary>
    public interface ILoggerFactory
    {
        /// <summary>
        /// Gets a logger with the specified name
        /// </summary>
        /// <param name="name">name of the logger</param>
        /// <returns>ILogger instance</returns>
        ILogger GetLogger(string name);

        /// <summary>
        /// Gets a logger for the specified Type
        /// </summary>
        /// <param name="type">type of the class that requested the logger</param>
        /// <returns>ILogger instance</returns>
        ILogger GetLogger(Type type);
    }
}
