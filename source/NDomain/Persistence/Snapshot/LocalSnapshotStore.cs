using System.Threading.Tasks;
using NDomain.Model;

namespace NDomain.Persistence.Snapshot
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
