using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.Model.Snapshot
{
    public interface ISnapshotStore
    {
        Task<IState> Load(string id);
        Task Save(string id, IState state, int expectedVersion);
    }
}
