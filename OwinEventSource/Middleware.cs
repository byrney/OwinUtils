namespace OwinEventSource
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.Owin;
    using EnvDict = System.Collections.Generic.IDictionary<string, object>;
    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>; 

    public class Middleware
    {
        AppFunc downstream;
        string environmentKey;

        public Middleware(AppFunc next, string environmentKey = "reponse.stream")
        {
            if (next == null) {
                throw new InvalidOperationException("Next middleware cannot be null");
            }
            this.downstream = next;
            this.environmentKey = environmentKey;
        }

        public Task Invoke(EnvDict env)
        {
            return this.Invoke(new OwinContext(env));
        }

        public Task Invoke(IOwinContext context)
        {
            var responseStream = context.Environment["owin.ResponseBody"] as Stream;
            var eventStream = new EventStream(responseStream, () => SetEventHeaders(context.Response));
            context.Environment[this.environmentKey] = eventStream;
            return this.downstream.Invoke(context.Environment);
        }

        private static void SetEventHeaders(object responseState)
        {
            //todo: this is probably too specific to SSE to go here
            var response = (IOwinResponse)responseState;
            response.ContentType = "text/eventstream";
            response.Headers["Transfer-Encoding"] = "chunked";
            response.Headers["cache-control"] = "no-cache";
        }

    }
}

