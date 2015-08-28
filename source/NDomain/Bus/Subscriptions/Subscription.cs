using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            this.Id = string.Format("{0}/{1}/{2}", topic, endpoint, component);
        }

        /// <summary>
        /// Topic name. Usually the name of a message
        /// </summary>
        public string Topic { get; private set; }

        /// <summary>
        /// Endpoint name. Usually the logical name of a process
        /// </summary>
        public string Endpoint { get; private set; }

        /// <summary>
        /// Component name within the endpoint. Usually the name of the handler that subscribes the message.
        /// </summary>
        /// <remarks>
        /// If one endpoint has multiple handlers (multiple components), each will have its own subscription and receive a copy of the message.
        /// </remarks>
        public string Component { get; private set; }

        public string Id { get; private set; }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (Subscription.ReferenceEquals(this, obj))
            {
                return true;
            }

            var other = obj as Subscription;
            return this.Id.Equals(other.Id);
        }

        static readonly char[] Separator = new char[] { '/' };
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
