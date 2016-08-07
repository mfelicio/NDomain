using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.CQRS.Projections
{
    /// <summary>
    /// In-memory and InProc implementation of a QueryStore
    /// </summary>
    /// <typeparam name="T"></typeparam>
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

        public async Task<Query<T>> GetOrWaitUntil(string id, int minExpectedVersion, TimeSpan timeout)
        {
            var query = await Get(id);

            if (query.Version >= minExpectedVersion)
            {
                return query;
            }

            var sw = Stopwatch.StartNew();
            do
            {
                await Task.Delay(5); //wait 5ms , should be exponential
                query = await Get(id);
            } while (query.Version < minExpectedVersion && sw.Elapsed < timeout);

            sw.Stop();

            return query;
        }

        public Task Set(string id, Query<T> query)
        {
            this.data[id] = query;
            return Task.FromResult(true);
        }
    }
}
