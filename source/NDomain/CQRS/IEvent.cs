using System;

namespace NDomain.CQRS
{
    /// <summary>
    /// Represents an event message
    /// </summary>
    public interface IEvent
    {
        /// <summary>
        /// Date in UTC when the event occurred
        /// </summary>
        DateTime DateUtc { get; }

        /// <summary>
        /// Name of the message, usually the name of the Payload's Type
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Payload of the event
        /// </summary>
        object Payload { get; }
    }

    /// <summary>
    /// Represents an event message with generic payload support
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IEvent<T> : IEvent
    {
        /// <summary>
        /// Payload of the event
        /// </summary>
        new T Payload { get; }
    }
}
