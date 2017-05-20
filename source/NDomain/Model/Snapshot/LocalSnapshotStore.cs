using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.Model.Snapshot
{
    public class LocalSnapshotStore : ISnapshotStore
    {
        public Task<IState> Load(string id)
        {
            return Task.FromResult<IState>(null);
        }

        public Task Save(string id, IState state, int expectedVersion)
        {
            return Task.FromResult(1);
        }
    }
}
