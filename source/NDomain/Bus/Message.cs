using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.Bus
{
    /// <summary>
    /// Represents an immutable generic message with support for headers, name and a payload
    /// </summary>
    public class Message
    {
        static readonly IReadOnlyDictionary<string, string> EmptyHeaders = new Dictionary<string, string>();

        public Message(object payload, string name, Dictionary<string, string> headers = null)
        {
            this.Payload = payload;
            this.Name = name;
            this.Headers = headers ?? EmptyHeaders;
        }

        public object Payload { get; private set; }
        public string Name { get; private set; }
        public IReadOnlyDictionary<string, string> Headers { get; private set; }
    }
}
