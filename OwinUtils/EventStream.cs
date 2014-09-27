namespace OwinUtils
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

        private bool Close(Exception e)
        {
            if (e == null || e.HResult == -2146232800) { // client closed the  connection
                this.tcs.SetResult(true);
            } else {
                this.tcs.SetException(e);
            }
            if (this.closeCallback != null)
                this.closeCallback.Invoke();
//            this.responseWriter.Dispose();
            return true; // exception handled
        }

        public void Close()
        {
            this.Close(null);
        }

        private async Task WriteAndFlush(string message)
        {
            await this.responseWriter.WriteAsync(message);
            await this.responseWriter.FlushAsync();
        }

        public void WriteAsync(string message)
        {
            try {
                this.WriteAndFlush(message).ContinueWith(t => t.Exception.Handle(Close));
            } catch (Exception e) {
                this.Close(e);
            }
        }

    }
}

