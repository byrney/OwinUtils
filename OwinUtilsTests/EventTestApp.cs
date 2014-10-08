namespace OwinUtilsTests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Net.Http;
    using Microsoft.Owin;
    using Owin;
    using OwinUtils;
    using EnvDict = System.Collections.Generic.IDictionary<string, object>;
    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;


    public class EventTestApp
    {
        public int onCloseCalledCount = 0;
        public string responseBody = "Hello Events";
        public string environmentKey = "response.events";
        public IEventStream stream = null;
        public IOwinContext context = null;
        public int writeDelayMilliseconds = 200;

        public EventTestApp()
        {
            Console.WriteLine("Startup"); 
        }

        private Task writeContextResponse(IOwinContext context)
        {
            return context.Response.WriteAsync(responseBody);
        }

        private void onClose()
        {
            onCloseCalledCount += 1;
        }

        private Task openClose(IOwinContext context)
        {
            var es = (IEventStream) context.Environment[environmentKey];
            var task = es.Open(onClose);
            es.Close();
            return task;
        }

        private void writeStream(object stream)
        {
            var s = stream as IEventStream;
            s.WriteAsync(this.responseBody + "\n");
        }

        private Task openReturnWrite(IOwinContext ctx)
        {
            var es = (IEventStream) ctx.Environment[environmentKey];
            var task = es.Open(onClose);
         //   writeStreamResponse(es);
            var timer = new System.Threading.Timer(writeStream, es, writeDelayMilliseconds, System.Threading.Timeout.Infinite);
            return task;
        }

        public void noEvents(IAppBuilder builder)
        {
            builder.Use<EventSource>(environmentKey);
            builder.Run(this.writeContextResponse);
        }

        public Task captureStream(IOwinContext ctx)
        {
            this.context = ctx;
            this.stream = ctx.Environment[environmentKey] as IEventStream;
            return this.stream.Open(onClose);
        }

        public Task openStreamAndWrite(IOwinContext ctx)
        {
            this.context = ctx;
            this.stream = ctx.Environment[environmentKey] as IEventStream;
            var task =  this.stream.Open(onClose);
            this.stream.WriteAsync(this.responseBody + "\n");
            return task;
        }

        public void openClose(IAppBuilder builder)
        {
            builder.Use<EventSource>(environmentKey);
            builder.Run(this.openClose);
        }


        public void openStream(IAppBuilder builder)
        {
            builder.Use<EventSource>(environmentKey);
            builder.Run(this.captureStream);
        }

        public void openStreamAndWrite(IAppBuilder builder)
        {
            builder.Use<EventSource>(environmentKey);
            builder.Run(this.openStreamAndWrite);
        }


        public void openStreamReturnAndWrite(IAppBuilder builder)
        {
            builder.Use<EventSource>(environmentKey);
            builder.Run(this.openReturnWrite);
        }

    }
}

