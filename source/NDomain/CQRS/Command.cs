namespace NDomain.CQRS
{
    public class Command<T> : ICommand<T>
    {
        public Command(string id, T payload)
            : this(id, typeof(T).Name, payload)
        {

        }

        public Command(string id, string name, T payload)
        {
            this.Id = id;
            this.Name = name;
            this.Payload = payload;
        }

        public string Id { get; }
        public string Name { get; }
        public T Payload { get; }

        object ICommand.Payload => this.Payload;
    }
}
