
OwinEventSource
==============

An Owin middleware that can be used to stream events back to the client (e.g HTML5 EventSource).

Add the middleware into your Owin pipeline
     
        public void Configuration(IAppBuilder app)
        {
            string envKey = "test.eventstream";
            app.EventSource(envKey);
            //....
        }

In your application (or downstream middleware) get the EventStream object from the Owin Environment
and return the Task from EventStream.Open()

	public Task Invoke(EnvDict env)
	{
		var eventStream = env["test.eventstream"] as IEventStream;
                Console.WriteLine("Got eventstream");
		// ....
                return eventStream.Open(() => Console.WriteLine("Closed"));
        }

Store the event-stream in a member variable/closure and then write to it later on

        public void Configuration(IAppBuilder app)
        {
            // chose a key in the environment to hold the stream
            string envKey = "test.eventstream";
            // add the middleware before your app
            app.Use<OwinUtils.EventSource>(envKey);
            // add the app
            app.Run(context => {
                // extract the stream from the env
                var eventStream = context.Environment[envKey] as IEventStream;
                // create a timer to write to the stream in 5 seconds
                var timer = new System.Threading.Timer(_ => {
                    eventStream.WriteAsync("message 1");
                    eventStream.Close();
                }, null, 5000,  System.Threading.Timeout.Infinite);
		// open the stream and return the task
                return eventStream.Open(() => Console.WriteLine("Closed"));
            });
        }



See Api Documentation: [Here](http://byrney.github.io/OwinUtils/doc/)
