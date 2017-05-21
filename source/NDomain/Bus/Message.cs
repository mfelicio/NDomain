using System.Collections.Generic;

namespace NDomain.Bus
{
    /// <summary>
    /// Represents an immutable generic message with support for headers, name and a payload
    /// </summary>
    public class Message
    {
        private static readonly IReadOnlyDictionary<string, string> EmptyHeaders = new Dictionary<string, string>();

        public Message(object payload, string name, Dictionary<string, string> headers = null)
        {
            this.Payload = payload;
            this.Name = name;
            this.Headers = headers ?? EmptyHeaders;
        }

        public object Payload { get; }
        public string Name { get; }
        public IReadOnlyDictionary<string, string> Headers { get; }
    }
}
