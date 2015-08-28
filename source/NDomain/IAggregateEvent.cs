using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain
{
    public interface IAggregateEvent
    {
        string AggregateId { get; }
        int SequenceId { get; }

        DateTime DateUtc { get; }
        string Name { get; }
        object Payload { get; }
    }

    public interface IAggregateEvent<T> : IAggregateEvent
    {
        new T Payload { get; }
    }
}
