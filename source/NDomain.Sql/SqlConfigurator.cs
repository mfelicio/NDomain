using NDomain.Configuration;
using NDomain.Sql.EventSourcing;

namespace NDomain.Sql
{
    public static class SqlConfigurator
    {
        public static EventSourcingConfigurator WithSqlServerStorage(
            this EventSourcingConfigurator configurator,
            string connectionString)
        {
            configurator.EventStoreDb = new SqlEventStoreDb(connectionString);
            return configurator;
        }

        public static EventSourcingConfigurator WithSqlServerStorage(
            this EventSourcingConfigurator configurator,
            string connectionString,
            SqlObjectNames names)
        {
            configurator.EventStoreDb = new SqlEventStoreDb(connectionString, names);
            return configurator;
        }
    }
}
