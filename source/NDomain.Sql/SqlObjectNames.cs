namespace NDomain.Sql
{
    public class SqlObjectNames
    {
        public string SchemaName { get; set; }
        public string AggregateTableName { get; set; }
        public string EventTableName { get; set; }

        public SqlObjectNames(string schemaName, string aggregateTableName, string eventTableName)
        {
            SchemaName = schemaName;
            AggregateTableName = aggregateTableName;
            EventTableName = eventTableName;
        }
    }
}
