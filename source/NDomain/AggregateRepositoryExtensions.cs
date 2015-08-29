using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain
{
    public static class AggregateRepositoryExtensions
    {
        public static async Task<T> Update<T>(this IAggregateRepository<T> repository, string id, Action<T> handler)
            where T : IAggregate
        {
            var aggregate = await repository.Find(id);

            handler(aggregate);

            await repository.Save(aggregate);

            return aggregate;
        }

        public static async Task<T> CreateOrUpdate<T>(this IAggregateRepository<T> repository, string id, Action<T> handler)
            where T : IAggregate
        {
            var aggregate = await repository.FindOrDefault(id);

            handler(aggregate);

            await repository.Save(aggregate);

            return aggregate;
        }
    }
}
