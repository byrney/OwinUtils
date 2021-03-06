﻿namespace OwinUtils
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    class EventStream : IEventStream
    {
        private Stream responseStream;
        private AsyncWriter responseWriter;
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
            this.responseWriter = new AsyncWriter(this.responseStream, Close);
            if(this.openCallback != null) {
                this.openCallback.Invoke();
            }
            return this.tcs.Task;
        }

        private bool IsSocketClosed(Exception e)
        {
            return e.HResult == -2146232800 || e.HResult == -2146233087;
        }

        private bool Close(Exception e)
        {
            if (e == null || IsSocketClosed(e)) { // client closed the  connection
                this.tcs.SetResult(true);
            } else {
                Console.WriteLine("Exception sending events: {0}", e);
                this.tcs.SetException(e);
            }
            if (this.closeCallback != null)
                this.closeCallback.Invoke();
        //    this.responseWriter.Dispose();
            return true; // exception handled
        }

        public void Close()
        {
            this.Close(null);
        }

        public Task WriteAsync(string message)
        {
            var w = this.responseWriter;
            return w.WriteAndFlushAsync(message);
        }

    }
}

