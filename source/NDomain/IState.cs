using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain
{
    public interface IState
    {
        int Version { get; }

        void Mutate(IAggregateEvent @event);
    }
}
