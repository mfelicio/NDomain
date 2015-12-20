using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain
{
    /// <summary>
    /// Aggregate event with generic payloads
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AggregateEvent<T> : IAggregateEvent<T>
    {
        readonly string aggregateId;
        readonly int sequenceId;
        readonly DateTime dateUtc;
        readonly string name;
        readonly T payload;

        public AggregateEvent(string aggregateId, int sequenceId, DateTime dateUtc, string name, T payload)
        {
            this.aggregateId = aggregateId;
            this.sequenceId = sequenceId;

            this.dateUtc = dateUtc;
            this.name = name;
            this.payload = payload;
        }

        public AggregateEvent(string aggregateId, int sequenceId, DateTime dateUtc, T payload)
            : this(aggregateId, sequenceId, dateUtc, typeof(T).Name, payload)
        {

        }

        public string AggregateId { get { return this.aggregateId; } }
        public int SequenceId { get { return this.sequenceId; } }

        public DateTime DateUtc { get { return this.dateUtc; } }
        public string Name { get { return this.name; } }
        public T Payload { get { return this.payload; } }

        object IAggregateEvent.Payload
        {
            get { return this.payload; }
        }

    }
}
