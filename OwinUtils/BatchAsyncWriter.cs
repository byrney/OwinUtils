using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OwinUtils
{
    public class BatchAsyncWriter
    {
        AsyncWriter writer;
        List<string> buffer;
        object bufferLock = new object();
        Func<Exception, bool> errorHandler;
        int batchSize;

        public BatchAsyncWriter(Stream dest, Func<Exception, bool> errorHandler, int batchSize = 100)
        {
            this.writer = new AsyncWriter(dest, errorHandler);
            this.batchSize = batchSize;
            this.buffer = null;
        }

        public Task Flush()
        {
            return this.writer.Flush();
        }

        public Task WriteAsync(string message)
        {
            lock (bufferLock) {
                if(this.buffer == null) {
                    this.buffer = new List<string>();
                }
                this.buffer.Add(message);
                if(this.buffer.Count >= batchSize) {
                    return Send();
                }
                return Task.FromResult<bool>(true);
            }
        }

        private Task Send()
        {
            var toSend = buffer;
            buffer = null;
            if(toSend == null) {
                return this.writer.Finished();
            }
            var message = String.Join("", toSend);
            return this.writer.WriteAsync(message);
        }

        public Task Finished()
        {
            lock (this.bufferLock) {
                return Send();
            }
        }

        public int Remaining { get 
            {
                lock(this.bufferLock) {
                return this.buffer != null ? this.buffer.Count : 0;
                }
            }
        }

    }
}

