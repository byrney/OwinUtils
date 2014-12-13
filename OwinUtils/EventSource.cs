using Owin;
using System;
using System.IO;
using System.Threading.Tasks;
using EnvDict = System.Collections.Generic.IDictionary<string, object>;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>; 
using HeaderDict = System.Collections.Generic.IDictionary<string, string[]>;


namespace OwinUtils
{
    /// <summary>
    /// Inbound: Inserts an IEvenStream object into the Owin Environment using the key passed 
    /// to the connstructor. See IEventStream for details on use.
    /// 
    /// Outbound: Sets HTML5 EventSource headers on the response. Whilst the IEventStream is open
    /// Data written to it will be added to the body.
    /// </summary>
    public static class AppBuilderEventSourceExtensions
    {
        public static IAppBuilder EventSource(this IAppBuilder iab, string environmentKey)
        {
            return iab.Use<EventSource>(environmentKey);
        }
    }

 
    internal class EventSource
    {
        AppFunc downstream;
        string environmentKey;

        public EventSource(AppFunc next, string environmentKey = "reponse.stream")
        {
            if (next == null) {
                throw new InvalidOperationException("Next middleware cannot be null");
            }
            this.downstream = next;
            this.environmentKey = environmentKey;
        }

        public Task Invoke(EnvDict env)
        {
            var responseStream = env["owin.ResponseBody"] as Stream;
            var eventStream = new EventStream(responseStream, () => SetEventHeaders(env));
            env[this.environmentKey] = eventStream;
            return this.downstream.Invoke(env);
        }

        private static void SetEventHeaders(EnvDict env)
        {
            //todo: this is probably too specific to SSE to go here
            var headers = (HeaderDict)env["owin.ResponseHeaders"];
            headers["Content-Type"] = new string[] {"text/event-stream"};
            headers["Transfer-Encoding"] = new string[] {"chunked"};
            headers["cache-control"] = new string[]{"no-cache"};
        }
    }



}

