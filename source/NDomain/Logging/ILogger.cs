using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.Logging
{
    public interface ILogger
    {
        void Debug(string message, params object[] args);

        void Info(string message, params object[] args);
        
        void Warn(string message, params object[] args);
        void Warn(Exception exception, string message, params object[] args);
        
        void Error(string message, params object[] args);
        void Error(Exception exception, string message, params object[] args);

        void Fatal(string message, params object[] args);
        void Fatal(Exception exception, string message, params object[] args);
    }
}
