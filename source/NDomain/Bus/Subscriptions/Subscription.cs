namespace NDomain.Bus.Subscriptions
{
    /// <summary>
    /// Subscription information for a specific message.
    /// </summary>
    public class Subscription
    {
        public Subscription(string topic, string endpoint, string component)
        {
            this.Topic = topic;
            this.Endpoint = endpoint;
            this.Component = component;

            this.Id = $"{topic}/{endpoint}/{component}";
        }

        /// <summary>
        /// Topic name. Usually the name of a message
        /// </summary>
        public string Topic { get; }

        /// <summary>
        /// Endpoint name. Usually the logical name of a process
        /// </summary>
        public string Endpoint { get; }

        /// <summary>
        /// Component name within the endpoint. Usually the name of the handler that subscribes the message.
        /// </summary>
        /// <remarks>
        /// If one endpoint has multiple handlers (multiple components), each will have its own subscription and receive a copy of the message.
        /// </remarks>
        public string Component { get; }

        public string Id { get; }

        public override int GetHashCode() => this.Id.GetHashCode();

        public override bool Equals(object obj)
        {
            if (Subscription.ReferenceEquals(this, obj))
            {
                return true;
            }

            var other = obj as Subscription;
            return this.Id.Equals(other.Id);
        }

        private static readonly char[] Separator = new char[] { '/' };
        public static Subscription FromId(string id)
        {
            var parts = id.Split(Separator, 3);

            var topic = parts[0];
            var endpoint = parts[1];
            var component = parts[2];

            return new Subscription(topic, endpoint, component);
        }
    }
}
