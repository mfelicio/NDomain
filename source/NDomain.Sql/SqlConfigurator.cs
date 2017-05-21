using NDomain.Configuration;
using NDomain.Sql;
using NDomain.Sql.Model.EventSourcing;

// ReSharper disable once CheckNamespace
namespace NDomain.Configurator
{
    public static class SqlConfigurator
    {
        public static ModelConfigurator WithSqlServerStorage(
            this ModelConfigurator configurator,
            string connectionString)
        {
            return configurator.WithSqlServerStorage(connectionString, SqlObjectNames.Default);
        }

        public static ModelConfigurator WithSqlServerStorage(
            this ModelConfigurator configurator,
            string connectionString,
            SqlObjectNames names)
        {
            configurator.EventStoreDb = new SqlEventStoreDb(connectionString, names);
            return configurator;
        }
    }
}
