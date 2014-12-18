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
using OwinUtils.Samples;


namespace ConsoleHost
{

    class Program
    {
 

        public static void SessionConfiguration(IAppBuilder app)
        {
            string envKey = "test.session";
            string passPhrase = "a pass phrase that is long";
            app.SessionCookie(envKey, passPhrase);
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
            builder.Map("/eventsource", EventSourceSample.BuildSample);
            builder.Map("/routeheader", RouteHeaderSample.BuildSample);
            builder.Map("/routequery", RouteQuerySample.BuildSample);
            builder.Map("/routeparams", RouteParamsSample.BuildSample);
            builder.Map("/branchsample", RouteBranchSample.BuildSample);
        }

        static void Main(string[] args)
        {
            var url = "http://localhost:12345";
            using (WebApp.Start(url, CombinedConfig))
            {
                Console.WriteLine("Listening on {0}", url);
                Console.WriteLine("Press return to quit");
                Console.ReadLine();
            }
        }
    }




}
