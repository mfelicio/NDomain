using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain
{
    internal interface IStateMutator
    {
        void Mutate(IState state, IAggregateEvent @event);
    }
}
