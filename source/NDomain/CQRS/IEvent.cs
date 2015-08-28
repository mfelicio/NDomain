using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.CQRS
{
    public interface IEvent
    {
        DateTime DateUtc { get; }
        string Name { get; }
        object Payload { get; }
    }

    public interface IEvent<T> : IEvent
    {
        new T Payload { get; }
    }
}
