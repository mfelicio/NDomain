using System;

namespace NDomain.Sql.EventSourcing
{
    public class EventEntity
    {
        public string aggregate_id { get; set; }
        public string aggregate_type { get; set; }
        public int event_seq { get; set; }
        public DateTime timestamp { get; set; }
        public string msg_type { get; set; }
        public int msg_ver { get; set; }
        public string msg_data { get; set; }
        public bool committed { get; set; }
        public string transaction_id { get; set; }
    }
}
