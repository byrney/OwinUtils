

namespace OwinUtils
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    /// <summary>
    /// Interface to write HTML5 EventSource style messages back to the caller
    /// Use The EventSourceBuilder methods to set up the middleware and inject an IEventStream
    /// into the Owin Environment. Capture  (IEventStream.Open) the stream in a downstream method
    /// and then use it to send messages to the client while the connection is kept open.
    /// 
    /// The onClose Action passed to the Open method can be used to clean up any state when the client drops
    /// the connection
    /// </summary>
    public interface IEventStream
    {
        /// <summary>
        /// Call this to capture the stream. Return the Task returned by this method in your Invoke
        /// to keep the stream open
        /// </summary>
        /// <param name="onClose">callback for when the stream is closed (either by client or server).</param>
        Task Open(Action onClose);
        /// <summary>
        /// Explicitly closes the stream from the server-side. 
        /// onClose callbacks will be called after the client is disconnected
        /// </summary>
        void Close();
        /// <summary>
        /// Write back to the client connection. This call should be thread-safe. Calls will be queued up for writing to the client
        /// and the stream will be flushed as soon as there are no more messages to send.
        /// </summary>
        /// <param name="message">Text to send</param>
        Task WriteAsync(string message);
    }
}

