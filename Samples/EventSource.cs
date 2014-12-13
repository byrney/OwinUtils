using System;
using OwinUtils;
using Owin;
using Microsoft.Owin;

namespace OwinUtils.Samples
{
    public class EventSource
    {
        static string block = "abcdefghijklmnopqrstuvwxyz";
        static string message = string.Format("{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}", block);
        static string envKey = "test.eventstream";

        public static void BuildSample(IAppBuilder app)
        {
 
            app.EventSource(envKey);
            app.Run(context => {
                var eventStream = context.Environment[envKey] as IEventStream;
                var timer = new System.Threading.Timer(_ => {
                    var ts = DateTime.UtcNow.ToString("O");
                    eventStream.WriteAsync(ts + message + "\n");
                }, null, 1,  50);
                var timer2 = new System.Threading.Timer(_ => {
                    var ts = DateTime.UtcNow.ToString("O");
                    eventStream.WriteAsync(ts + "\n");
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

    }
}

