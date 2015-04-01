
using System;
using System.IO;
using System.Threading.Tasks;
using System.Text;

namespace OwinUtils
{
    public class AsyncWriter : IDisposable
    {
        Task currentWrite;
        Object currentLock = new object();
        StreamWriter writer;
        Func<Task> FlushFunc;
        Func<Exception, bool> errorHandler;
        int pending = 0;
        int maxPending = 10;
        static Encoding utf8unmarked = new UTF8Encoding(false, true);

        public AsyncWriter(Stream dest, Func<Exception, bool> errorHandler, int bufferSize = 4096)
        {
            this.writer = new StreamWriter(dest, utf8unmarked, bufferSize);
            this.currentWrite = null;
            this.FlushFunc = FlushIfNoWritesPending;
            this.errorHandler = errorHandler;
        }

        private void OnError(Task t)
        {
            t.Exception.Handle(errorHandler);
        }

        public void Dispose()
        {
            this.writer.Dispose();
        }

        private  const TaskContinuationOptions OnFaulted =
            TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously;
        private const TaskContinuationOptions OnSuccess =
            TaskContinuationOptions.NotOnFaulted | TaskContinuationOptions.ExecuteSynchronously;

        public Task FlushIfNoWritesPending()
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
                    currentWrite = Then(currentWrite, FlushFunc);
                }
                return currentWrite;
            }
        }

        public Task Finished()
        {
            return currentWrite;
        }

        public static Task Then(Task first, Func<Task> next)
        {
            var tcs = new TaskCompletionSource<object>();
            first.ContinueWith(_ =>
            {
                if (first.IsFaulted){
                    tcs.TrySetException(first.Exception.InnerExceptions);
                }
                else if (first.IsCanceled){
                    tcs.TrySetCanceled();
                }
                else
                {
                    try
                    {
                        next().ContinueWith(t =>  {
                            if (t.IsFaulted) {
                                tcs.TrySetException(t.Exception.InnerExceptions);
                            }
                            else if (t.IsCanceled) {
                                tcs.TrySetCanceled();
                            }
                            else {
                                tcs.TrySetResult(null);
                            }
                        }, TaskContinuationOptions.ExecuteSynchronously);
                    }
                    catch (Exception exc) { tcs.TrySetException(exc); }
                }
            }, TaskContinuationOptions.ExecuteSynchronously);
            return tcs.Task;
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

        private Task WriteMore(Task prev, object state)
        {
            return writer.WriteAsync((string)state);
        }

        public Task WriteAsync(string message)
        {
            Func<Task> more = () => writer.WriteAsync(message);
            lock (currentLock) {
                if(this.pending > this.maxPending) {
                    currentWrite.Wait();
                }
                if(currentWrite == null || currentWrite.IsCompleted) {
                    pending = 0;
                    currentWrite = writer.WriteAsync(message);
                } else {
                    pending += 1;
                    currentWrite = Then(currentWrite, more);
                }
                return currentWrite;
            }
        }

        public Task WriteAndFlushAsync(string message)
        {
            lock (currentLock) {
                currentWrite = WriteAsync(message);
                currentWrite = Then(currentWrite, FlushFunc);
                return currentWrite;
            }
        }

    }
}

