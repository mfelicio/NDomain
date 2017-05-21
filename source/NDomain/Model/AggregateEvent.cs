using System;

namespace NDomain.Model
{
    /// <summary>
    /// Aggregate event with generic payloads
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AggregateEvent<T> : IAggregateEvent<T>
    {
        public AggregateEvent(string aggregateId, int sequenceId, DateTime dateUtc, string name, T payload)
        {
            this.AggregateId = aggregateId;
            this.SequenceId = sequenceId;
            this.DateUtc = dateUtc;
            this.Name = name;
            this.Payload = payload;
        }

        public AggregateEvent(string aggregateId, int sequenceId, DateTime dateUtc, T payload)
            : this(aggregateId, sequenceId, dateUtc, typeof(T).Name, payload)
        {

        }

        public string AggregateId { get; }
        public int SequenceId { get; }
        public DateTime DateUtc { get; }
        public string Name { get; }
        public T Payload { get; }

        object IAggregateEvent.Payload => this.Payload;
    }
}
