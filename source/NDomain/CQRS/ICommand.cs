using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.CQRS
{
    /// <summary>
    /// Represents a command message
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Id of the command
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Name of the command
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Payload of the command
        /// </summary>
        object Payload { get; }
    }

    /// <summary>
    /// Represents a command message with generic payload
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICommand<T> : ICommand
    {
        /// <summary>
        /// Payload of the command
        /// </summary>
        new T Payload { get; }
    }
}
