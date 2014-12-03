using System;
using Microsoft.Owin.Hosting;
using Owin;
using OwinUtils;
using System.Collections.Generic;
using HeaderDict = System.Collections.Generic.IDictionary<string, string[]>;
using EnvDict = System.Collections.Generic.IDictionary<string, object>;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;
using System.Threading.Tasks;
using Microsoft.Owin;

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
        public static void EventConfiguration(IAppBuilder app)
        {
            string block = "abcdefghijklmnopqrstuvwxyz";
            string message = string.Format("{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}", block);
            string envKey = "test.eventstream";
            app.Use<EventSource>(envKey);
            app.Run(context => {
                var eventStream = context.Environment[envKey] as IEventStream;
                var timer = new System.Threading.Timer(_ => {
                    var ts = DateTime.UtcNow.ToString("O");
                    eventStream.WriteAsync(ts + message + "\n");
                    //        eventStream.Close();
                }, null, 1,  50);
                var timer2 = new System.Threading.Timer(_ => {
                    var ts = DateTime.UtcNow.ToString("O");
                    eventStream.WriteAsync(ts + "\n");
                    //        eventStream.Close();
                }, null, 1,  25);
                var task =  eventStream.Open(() => {
                    Console.WriteLine("Closed");
                    timer.Dispose();
                    timer2.Dispose();
                });
                Console.WriteLine("Got eventstream");

                eventStream.WriteAsync("Started\n");
                return task;
            });
        }

        public static void SessionConfiguration(IAppBuilder app)
        {
            string envKey = "test.session";
            string passPhrase = "a pass phrase that is long";
            app.Use<SessionCookie>(envKey, passPhrase);
            app.Run(context => {
                var inboundSession = context.Environment[envKey];
                Console.WriteLine("Inbound session: {0}", inboundSession);
                context.Environment[envKey] = "some session data";
                return context.Response.WriteAsync(string.Format("Hello: {0}\n", inboundSession));
            });
        }



        public static Task SayHelloC(IOwinContext ctx)
        {
            return ctx.Response.WriteAsync("hello");
        }

        public static Task SayHello(EnvDict env, EnvDict routeParams)
        {
            var ctx = new OwinContext(env);
            return ctx.Response.WriteAsync("hello");
        }

        public static Task SayGoodbye(EnvDict env, EnvDict routeParams)
        {
            var ctx = new OwinContext(env);
            return ctx.Response.WriteAsync("Goodbye");
        }

        public static void BranchedConfiguration(IAppBuilder app)
        {
         //   app.Branch("hello", b => b.Run(SayHello));
           // app.Route("goodbye", SayGoodbye);
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

        static void CombinedConfig(IAppBuilder builder)
        {
            builder.Map("/events", EventConfiguration);
            builder.Map("/session", SessionConfiguration);
        }

        static void Main(string[] args)
        {
            var url = "http://localhost:12345";
            using (WebApp.Start(url, CombinedConfig))
            {
                Console.WriteLine("Listening on {0}", url);
                Console.ReadLine();
            }
        }
    }




}
