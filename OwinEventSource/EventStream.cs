namespace OwinEventSource
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    class EventStream : IEventStream
    {
        private Stream responseStream;
        private StreamWriter responseWriter;
        private TaskCompletionSource<bool> tcs;
        private Action closeCallback;
        private Action openCallback;

        public EventStream (Stream responseStream, Action onOpen)
        {
            this.responseStream = responseStream;
            this.openCallback = onOpen;
        }

        public Task Open(Action onClose)
        {
            this.closeCallback = onClose;
            this.tcs = new TaskCompletionSource<bool>();
            this.responseWriter = new StreamWriter(this.responseStream);
            if(this.openCallback != null) {
                this.openCallback.Invoke();
            }
            return this.tcs.Task;
        }

        private void Close(Exception e)
        {
            if (e == null || e.HResult == -2146232800) { // client closed the  connection
                this.tcs.SetResult(true);
            } else {
                this.tcs.SetException(e);
            }
            if (this.closeCallback != null)
                this.closeCallback.Invoke();
//            this.responseWriter.Dispose();
        }

        public void Close()
        {
            this.Close(null);
        }

        public void WriteAsync(string message)
        {
            try {
                this.responseWriter.Write(message);
                this.responseWriter.Flush();
            } catch (Exception e) {
                this.Close(e);
            }
        }

    }
}

