using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.CQRS
{
    public interface ICommandBus
    {
        Task Send(ICommand command);
        Task Send<T>(ICommand<T> command);
    }
}
