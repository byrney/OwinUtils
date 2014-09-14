using System;
using Microsoft.Owin.Hosting;
using Owin;

namespace ConsoleHost
{
    class Program
    {
        static void Main(string[] args)
        {
            using (WebApp.Start<Startup>("http://localhost:12345"))
            {
                Console.ReadLine();
            }
        }
    }

    // creates the pipeline  (of one)
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            string envKey = "test.session";
            string passPhrase = "a pass phrase that is long";
            app.Use<OwinSession.SessionMiddleware>(envKey, passPhrase);
            app.Run(context => {
                var inboundSession = context.Environment[envKey];
                Console.WriteLine("Inbound session: {0}", inboundSession);
                context.Environment[envKey] = "some session data";
                return context.Response.WriteAsync(string.Format("Hello: {0}\n", inboundSession));
            });
        }
    }
}
