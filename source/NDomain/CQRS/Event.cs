using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.CQRS
{
    public class Event<T> : IEvent<T>
    {
        readonly DateTime dateUtc;
        readonly string name;
        readonly T payload;

        public Event(DateTime dateUtc, T payload)
            : this(dateUtc, typeof(T).Name, payload)
        {

        }

        public Event(DateTime dateUtc, string name, T payload)
        {
            this.dateUtc = dateUtc;
            this.name = name;
            this.payload = payload;
        }

        public DateTime DateUtc { get { return this.dateUtc; } }
        public string Name { get { return this.name; } }
        public T Payload { get { return this.payload; } }

        object IEvent.Payload
        {
            get { return this.payload; }
        }
    }

}
