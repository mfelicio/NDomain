using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.CQRS.Sagas.Persistence
{
    public class SagaData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Version { get; set; }
        public JObject State { get; set; }
        public bool Completed { get; set; }
    }
}
