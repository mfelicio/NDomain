using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.CQRS
{
    public class Command<T> : ICommand<T>
    {
        readonly string id;
        readonly string name;
        readonly T payload;

        public Command(string id, T payload)
            : this(id, typeof(T).Name, payload)
        {

        }

        public Command(string id, string name, T payload)
        {
            this.id = id;
            this.name = name;
            this.payload = payload;
        }

        public string Id { get { return this.id; } }
        public string Name { get { return this.name; } }
        public T Payload { get { return this.payload; } }

        object ICommand.Payload
        {
            get { return this.payload; }
        }
    }
}
