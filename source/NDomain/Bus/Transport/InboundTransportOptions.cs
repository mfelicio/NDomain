using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.Bus.Transport
{
    public class InboundTransportOptions
    {
        internal const string DeadLetterEndpointFormat = "{0}-deadletter";
        internal const int DefaultMaxDeliveryCount = 10;
        internal const bool DefaultDeadLetterMessages = true;

        public InboundTransportOptions(
            string endpoint, 
            int maxDeliveryCount = DefaultMaxDeliveryCount, 
            bool deadLetterMessages = DefaultDeadLetterMessages)
        {
            this.Endpoint = endpoint;
            this.MaxDeliveryCount = maxDeliveryCount;
            this.DeadLeterMessages = deadLetterMessages;
        }

        public string Endpoint { get; private set; }
        public int MaxDeliveryCount { get; private set; }
        public bool DeadLeterMessages { get; private set; }

        public string GetDeadLetterEndpoint()
        {
            return string.Format(DeadLetterEndpointFormat, Endpoint);
        }
    }
}
