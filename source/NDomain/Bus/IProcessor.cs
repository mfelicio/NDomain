using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.Bus
{
    public interface IProcessor : IDisposable
    {
        void Start();
        void Stop();

        bool IsRunning { get; }
    }
}