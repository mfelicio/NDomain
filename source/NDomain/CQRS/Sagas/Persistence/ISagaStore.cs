using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.CQRS.Sagas.Persistence
{
    public interface ISagaStore
    {
        Task<SagaData> Get(string sagaId);
        Task Set(SagaData data, int expectedVersion);
    }
}
