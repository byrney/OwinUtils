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
                    currentWrite.ContinueWith(FlushFunc, OnSuccess);
                }
                return currentWrite;

            }
        }

        public Task Flush()
        {
            lock(currentLock) {
                if(currentWrite == null || currentWrite.IsCompleted) {
                    return writer.FlushAsync();
                } else {
                    return currentWrite.ContinueWith(_ => writer.Flush())
                        //.ContinueWith(OnError, TaskContinuationOptions.OnlyOnFaulted)
                        ;
                }
            }
        }

        public Task WriteAndFlushAsync(string message)
        {
            Func<Task, Task> WriteFunc = t => writer.WriteAsync(message);
            lock (currentLock) {
                if(currentWrite == null || currentWrite.IsCompleted) {
                    currentWrite = WriteFunc(null);
                } else {
                    currentWrite = currentWrite.ContinueWith(WriteFunc, OnSuccess);
                }
                currentWrite.ContinueWith(OnError, OnFaulted).ContinueWith(FlushFunc, OnSuccess);
                return currentWrite;
            }
        }

    }
}

