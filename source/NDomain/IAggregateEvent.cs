using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain
{
    /// <summary>
    /// Represents an event inside an event stream that belongs to an aggregate. 
    /// There's an event stream per aggregate, identified by the AggregateId.
    /// Each event is uniquely identified in the event stream by its sequenceId
    /// </summary>
    public interface IAggregateEvent
    {
        /// <summary>
        /// Uniquely identifies the aggregate and the event stream on which the event belongs
        /// </summary>
        string AggregateId { get; }

        /// <summary>
        /// Uniquely identifies an event in the event stream.
        /// </summary>
        /// <remarks>This is also used to determine the version of the aggregate, which is the last sequenceId in the event stream</remarks>
        int SequenceId { get; }

        /// <summary>
        /// Date, in UTC, when the event occurred
        /// </summary>
        DateTime DateUtc { get; }

        /// <summary>
        /// Name of the event, usually the name of the Type of the event's Payload
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Actual Payload of the event. This is the event fired from the Aggregate
        /// </summary>
        object Payload { get; }
    }

    /// <summary>
    /// AggregateEvent interface with generics Payload support
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IAggregateEvent<T> : IAggregateEvent
    {
        /// <summary>
        /// Generic Payload
        /// </summary>
        new T Payload { get; }
    }
}
