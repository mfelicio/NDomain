using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain
{
    public interface IAggregateRepository<T>
            where T : IAggregate
    {
        IAggregateFactory<T> Factory { get; }

        Task<T> Find(string id);

        Task<T> FindOrDefault(string id);

        Task<T> Save(T aggregate);
    }
}
