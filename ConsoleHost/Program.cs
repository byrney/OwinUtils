using System;
using Microsoft.Owin.Hosting;
using Owin;
using OwinUtils;
using System.Collections.Generic;
using HeaderDict = System.Collections.Generic.IDictionary<string, string[]>;


static class DictionaryExtensions
{
    public static TValue GetValueOrDefault<TKey,TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
    {
        TValue ret;
        // Ignore return value
        dictionary.TryGetValue(key, out ret);
        return ret;
    }
}

namespace ConsoleHost
{

    class Program
    {
        public void EventConfiguration(IAppBuilder app)
        {
            string envKey = "test.eventstream";
            app.Use<EventSource>(envKey);
            app.Run(context => {
                var eventStream = context.Environment[envKey] as IEventStream;
                var task =  eventStream.Open(() => Console.WriteLine("Closed"));
                Console.WriteLine("Got eventstream");
                var timer = new System.Threading.Timer(_ => {
                    eventStream.WriteAsync("message 1\n");
                    eventStream.Close();
                }, null, 5000,  System.Threading.Timeout.Infinite);
                eventStream.WriteAsync("Started\n");
                return task;
            });
        }

        public static void SessionConfiguration(IAppBuilder app)
        {
            string envKey = "test.session";
            string passPhrase = "a pass phrase that is long";
            app.Use<Session>(envKey, passPhrase);
            app.Run(context => {
                var inboundSession = context.Environment[envKey];
                Console.WriteLine("Inbound session: {0}", inboundSession);
                context.Environment[envKey] = "some session data";
                return context.Response.WriteAsync(string.Format("Hello: {0}\n", inboundSession));
            });
        }

        public static void ForwardedConfiguration(IAppBuilder app)
        {
            app.Use<Forwarded>("/root");
            app.Run(context => {
                var fub = new ForwardedUrlBuilder(context.Environment);
                var url = fub.url(context.Request.Path.ToString());
                return context.Response.WriteAsync(string.Format("Hello from: {0}\n", url));
            });
        }

        public static string GetHeaderOrNull(IDictionary<string, string[]> dictionary, string key)
        {
            string[] ret = null;
            // Ignore return value
            dictionary.TryGetValue(key, out ret);
            return ret == null ? null : ret[0];
        }

        interface IUrlBuilder
        {
 
        }

        static void Main(string[] args)
        {
            var url = "http://localhost:12345";
            using (WebApp.Start(url, ForwardedConfiguration))
            {
                Console.WriteLine("Listening on {0}", url);
                Console.ReadLine();
            }
        }
    }

 

}
