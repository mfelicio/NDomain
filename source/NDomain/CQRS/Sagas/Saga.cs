using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.CQRS.Sagas
{
    public class Saga<T>
    {
        readonly string id;
        readonly T state;

        readonly List<ICommand> commands;

        public Saga(string id, T state)
        {
            this.id = id;
            this.state = state;
            this.commands = new List<ICommand>();
        }

        public string Id { get { return this.id; } }
        public T State { get { return this.state; } }

        internal bool Completed { get; private set; }
        internal IEnumerable<ICommand> Commands { get { return this.commands; } }

        public void Send(ICommand command)
        {
            this.commands.Add(command);
        }

        public void MarkAsCompleted()
        {
            this.Completed = true;
        }
    }
}
