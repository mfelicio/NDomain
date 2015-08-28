using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.CQRS.Projections
{
    public class LocalQueryStore<T> : IQueryStore<T>
    {
        private readonly ConcurrentDictionary<string, Query<T>> data;

        public LocalQueryStore()
        {
            this.data = new ConcurrentDictionary<string, Query<T>>();
        }

        public Task<Query<T>> Get(string id)
        {
            var query = this.data.GetOrAdd(id, i =>
                new Query<T>
                {
                    Id = id,
                    Version = 0,
                    DateUtc = DateTime.UtcNow,
                    Data = default(T)
                });

            return Task.FromResult(query);
        }

        public Task Set(string id, Query<T> query)
        {
            this.data[id] = query;
            return Task.FromResult(true);
        }
    }
}
