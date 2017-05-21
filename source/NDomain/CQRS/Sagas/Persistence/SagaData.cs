using Newtonsoft.Json.Linq;

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
