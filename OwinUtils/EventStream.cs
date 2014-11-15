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
     //       if (this.tcs.Task.Status != TaskStatus.Running) {
       //         return true;
         //   }
            if (e == null || e.HResult == -2146232800) { // client closed the  connection
                this.tcs.SetResult(true);
            } else {
                Console.WriteLine("Exception sending events: {0}", e);
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

        private Task WriteAndFlush(string message)
        {
            var w = this.responseWriter;
            return w.WriteAsync(message).ContinueWith(t => w.FlushAsync()
                , TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        public Task WriteAsync(string message)
        {
              return this.WriteAndFlush(message).ContinueWith(t => t.Exception.Handle(Close)
                , TaskContinuationOptions.OnlyOnFaulted);
        }

    }
}

