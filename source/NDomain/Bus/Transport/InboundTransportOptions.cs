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

        public string Endpoint { get; }
        public int MaxDeliveryCount { get; }
        public bool DeadLeterMessages { get; }

        public string GetDeadLetterEndpoint()
        {
            return string.Format(DeadLetterEndpointFormat, Endpoint);
        }
    }
}
