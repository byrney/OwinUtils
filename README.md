
OwinUtils [![Build Status](https://travis-ci.org/byrney/OwinUtils.svg?branch=master)](https://travis-ci.org/byrney/OwinUtils)
=========




Some midddleware to use with Owin.

* OwinEventSource: stream events back to client (HTML5 Server Sent Events)
* OwinSession: signed session cookies

See Api Documentation: [Here](http://byrney.github.io/OwinUtils/doc/)

OwinSession
===========

When placed upstream this will convert inbound cookies to a value in the
environment dictionary (checking that they are signed) and convert the
corresponding environment key into a signed cookie.


OwinEventSource
==============

An Owin middleware that can be used to stream events back to the
client (e.g HTML5 EventSource).

Add the middleware into your Owin pipeline

```cs
public void Configuration(IAppBuilder app)
{
    string envKey = "test.eventstream";
    app.Use<OwinUtils.EventSource>(envKey);
    //....
}
```

In your application (or downstream middleware) get the EventStream object
from the Owin Environment and return the Task from EventStream.Open()

```cs
public Task Invoke(EnvDict env)
{
    var eventStream = env["test.eventstream"] as IEventStream;
            Console.WriteLine("Got eventstream");
    // ....
            return eventStream.Open(() => Console.WriteLine("Closed"));
    }
```

Store the event-stream in a member variable/closure and then write
to it later on

```cs
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
```




