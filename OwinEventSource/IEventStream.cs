

namespace OwinEventSource
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    public interface IEventStream
    {
        // call this to capture the stream. onClose will be called when the stream
        // is closed (either by client or server).
        // The return Task should be returned from your Invoke() method to keep the stream open
        // The Task will not complete until the stream is closed
        Task Open(Action onClose);
        // Explicitly closes the stream from the server-side
        // onClose callbacks will be called after the client is disconnected
        void Close();
        // write a message to the stream
        void WriteAsync(string message);
    }
}

