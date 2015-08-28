using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.CQRS.Projections
{
    public interface IQueryStore<T>
    {
        Task<Query<T>> Get(string id);
        Task Set(string id, Query<T> query);
    }
}
