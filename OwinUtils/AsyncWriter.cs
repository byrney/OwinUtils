
using System;
using System.IO;
using System.Threading.Tasks;

namespace OwinUtils
{
    public class AsyncWriter
    {
        Task currentWrite;
        Object currentLock = new object();
        StreamWriter writer;
        Func<Task, Task> FlushFunc;
        Func<Exception, bool> errorHandler;

        public AsyncWriter(Stream dest, Func<Exception, bool> errorHandler)
        {
            this.writer = new StreamWriter(dest);
            this.currentWrite = null;
            this.FlushFunc = FlushIfNoWritesPending;
            this.errorHandler = errorHandler;
        }

        private void OnError(Task t)
        {
            t.Exception.Handle(errorHandler);
        }

        private  const TaskContinuationOptions OnFaulted =
            TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously;
        private const TaskContinuationOptions OnSuccess =
            TaskContinuationOptions.NotOnFaulted | TaskContinuationOptions.ExecuteSynchronously;

        public Task FlushIfNoWritesPending(Task ignored)
        {
            lock (currentLock) {
                if(currentWrite == null) {
                    return Task.FromResult<bool>(true);
                }
                if(currentWrite.IsCompleted) {
                    currentWrite = writer.FlushAsync()
                        .ContinueWith(OnError, OnFaulted)
                        ;
                } else {
                    currentWrite = currentWrite.ContinueWith(FlushFunc, OnSuccess);
                }
                return currentWrite;

            }
        }

        public Task Finished()
        {
            return currentWrite;
        }

        public Task Flush()
        {
            lock(currentLock) {
                if(currentWrite == null || currentWrite.IsCompleted) {
                    currentWrite = writer.FlushAsync();
                } else {
                    currentWrite = currentWrite.ContinueWith(_ => writer.Flush());
                }
                currentWrite.ContinueWith(OnError, OnFaulted);
                return currentWrite;
            }
        }

        public Task WriteAsync(string message)
        {
            lock (currentLock) {
                if(currentWrite == null || currentWrite.IsCompleted) {
                    currentWrite = writer.WriteAsync(message);
                } else {
                    var nextWrite = currentWrite.ContinueWith(_ => writer.WriteAsync(message), OnSuccess);
                    currentWrite = nextWrite;
                }
                return currentWrite;
            }
        }

        public Task WriteAndFlushAsync(string message)
        {
            lock (currentLock) {
                currentWrite = WriteAsync(message);
                currentWrite = currentWrite.ContinueWith(OnError, OnFaulted).ContinueWith(FlushFunc, OnSuccess);
                return currentWrite;
            }
        }

    }
}

