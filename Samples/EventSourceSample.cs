using System;
using OwinUtils;
using Owin;
using Microsoft.Owin;

namespace OwinUtils.Samples
{
    public class EventSourceSample
    {
        static string block = "abcdefghijklmnopqrstuvwxyz";
        static string message = string.Format("{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}", block);
        static string envKey = "test.eventstream";

        public static void BuildSample(IAppBuilder app)
        {
            // add eventsource middleware
            app.EventSource(envKey);
            app.Run(context => {

                // get the event stream (not captured yet)
                var eventStream = context.Environment[envKey] as IEventStream;

                // create some timers to send mesages
                var timer = new System.Threading.Timer(_ => {
                    var ts = DateTime.UtcNow.ToString("O");
                    eventStream.WriteAsync("Timer1:" + ts + message + "\n");
                }, null, 1,  50);
                var timer2 = new System.Threading.Timer(_ => {
                    var ts = DateTime.UtcNow.ToString("O");
                    eventStream.WriteAsync("Timer 2:" + ts + "\n");
                }, null, 1,  25);

                // Capture the eventstream by calling Open and pass in the 
                // clean-up logic for when this client closes the stream
                var task =  eventStream.Open(() => {
                    Console.WriteLine("Closed");
                    timer.Dispose();
                    timer2.Dispose();
                });

                eventStream.WriteAsync("Started\n");
                return task;
            });
        }

    }
}

