using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.CQRS
{
    public interface ICommand
    {
        string Id { get; }
        string Name { get; }
        object Payload { get; }
    }

    public interface ICommand<T> : ICommand
    {
        new T Payload { get; }
    }
}
