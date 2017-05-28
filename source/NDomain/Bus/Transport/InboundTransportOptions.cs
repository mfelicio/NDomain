namespace NDomain.Bus.Transport
{
    public class InboundTransportOptions
    {
        private const string DeadLetterEndpointFormat = "{0}-deadletter";
        private const bool DefaultDeadLetterMessages = true;

        internal const int DefaultMaxDeliveryCount = 10;

        public InboundTransportOptions(
            string endpoint, 
            int maxDeliveryCount = DefaultMaxDeliveryCount, 
            bool deadLetterMessages = DefaultDeadLetterMessages)
        {
            this.Endpoint = endpoint;
            this.MaxDeliveryCount = maxDeliveryCount;
            this.DeadLetterMessages = deadLetterMessages;
            this.DeadLetterEndpoint = string.Format(DeadLetterEndpointFormat, this.Endpoint);
        }

        public string Endpoint { get; }
        public int MaxDeliveryCount { get; }
        public bool DeadLetterMessages { get; }
        public string DeadLetterEndpoint { get; }
    }
}
