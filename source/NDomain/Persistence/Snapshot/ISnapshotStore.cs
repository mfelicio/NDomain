using System.Threading.Tasks;
using NDomain.Model;

namespace NDomain.Persistence.Snapshot
{
    public interface ISnapshotStore
    {
        Task<IState> Load(string id);
        Task Save(string id, IState state, int expectedVersion);
    }
}
