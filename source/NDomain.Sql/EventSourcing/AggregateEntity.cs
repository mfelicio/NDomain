namespace NDomain.Sql.EventSourcing
{
    public class AggregateEntity
    {
        public string aggregate_id { get; set; }
        public string aggregate_type { get; set; }
        public int aggregate_event_seq { get; set; }
        public int snapshot_event_seq { get; set; }
    }
}
