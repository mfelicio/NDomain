using System.Threading.Tasks;

namespace NDomain.CQRS
{
    /// <summary>
    /// Sends command messages in the bus
    /// </summary>
    public interface ICommandBus
    {
        /// <summary>
        /// Sends a command message
        /// </summary>
        /// <param name="command">command message</param>
        /// <returns>Task</returns>
        Task Send(ICommand command);

        /// <summary>
        /// Sends a command message
        /// </summary>
        /// <typeparam name="T">Type of the command's Payload</typeparam>
        /// <param name="command">command message</param>
        /// <returns>Task</returns>
        Task Send<T>(ICommand<T> command);
    }
}
