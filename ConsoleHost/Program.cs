using System;
using Microsoft.Owin.Hosting;
using Owin;
using OwinUtils;


namespace ConsoleHost
{
    class Program
    {
        static void Main(string[] args)
        {
            var url = "http://localhost:12345";
            using (WebApp.Start<EventPipeline>(url))
            {
                Console.WriteLine("Listening on {0}", url);
                Console.ReadLine();
            }
        }
    }

    // creates the pipeline  (of one) session middleware
    public class SessionPipeline
    {
        public void Configuration(IAppBuilder app)
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
    }

    // creates the pipeline  (of one) session middleware
    public class EventPipeline
    {
        public void Configuration(IAppBuilder app)
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
    }

}
