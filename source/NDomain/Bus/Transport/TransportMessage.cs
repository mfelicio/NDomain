using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.Bus.Transport
{
    public class TransportMessage
    {
        public TransportMessage()
        {
            this.Headers = new Dictionary<string, string>();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public JObject Body { get; set; }
        public Dictionary<string, string> Headers { get; set; }
    }
}
