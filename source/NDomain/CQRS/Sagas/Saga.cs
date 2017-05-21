using System.Collections.Generic;

namespace NDomain.CQRS.Sagas
{
    public class Saga<T>
    {
        readonly List<ICommand> commands;

        public Saga(string id, T state)
        {
            this.Id = id;
            this.State = state;
            this.commands = new List<ICommand>();
        }

        public string Id { get; }
        public T State { get; }

        internal IEnumerable<ICommand> Commands => this.commands;
        internal bool Completed { get; private set; }

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
