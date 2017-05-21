using System;

namespace NDomain.CQRS
{
    public class Event<T> : IEvent<T>
    {
        public Event(DateTime dateUtc, T payload)
            : this(dateUtc, typeof(T).Name, payload)
        {

        }

        public Event(DateTime dateUtc, string name, T payload)
        {
            this.DateUtc = dateUtc;
            this.Name = name;
            this.Payload = payload;
        }

        public DateTime DateUtc { get; }
        public string Name { get; }
        public T Payload { get; }

        object IEvent.Payload => this.Payload;
    }

}
