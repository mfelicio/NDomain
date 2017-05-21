using System;

namespace NDomain.Bus
{
    /// <summary>
    /// Processes incoming messages based on the subscriptions registered for the current process.
    /// Allows a process to subscribe and/or unsubscribe to specific messages
    /// </summary>
    public interface IProcessor : IDisposable
    {
        /// <summary>
        /// Starts processing incoming messages
        /// </summary>
        void Start();

        /// <summary>
        /// Stops processing incoming messages
        /// </summary>
        void Stop();

        /// <summary>
        /// Indicates whether the current processor is running and processing incoming messages
        /// </summary>
        bool IsRunning { get; }
    }
}