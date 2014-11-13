namespace OwinUtilsTests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Net.Http;
    using Microsoft.Owin;
    using Owin;
    using OwinUtils;
    using EnvDict = System.Collections.Generic.IDictionary<string, object>;
    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;


    public class SessionTestApp
    {
        public string envKey = "test.session";
        public string passPhrase = "a pass phrase that is long";
        public string responseBody = "goodbye\n";
        public string sessionData = "some data that is in the session";

        public object inboundSession = "default";

        public SessionTestApp()
        {
            Console.WriteLine("Startup"); 
        }

        public Task captureFunc(IOwinContext context)
        {
            var env = context.Environment;
            object inboundSession = env.ContainsKey(envKey) ? env[envKey] : null;
            context.Environment[envKey] = sessionData;
            this.inboundSession = inboundSession;
            return context.Response.WriteAsync(responseBody);
        }

        public Task setFunc(IOwinContext context)
        {
            context.Environment[envKey] = sessionData;
            return context.Response.WriteAsync(responseBody);
        }

        public void CaptureInbound(IAppBuilder builder)
        {
            builder.Use<SessionCookie>(envKey, passPhrase);
            builder.Run(this.captureFunc);
        }

        public void SetOutbound(IAppBuilder builder)
        {
            builder.Use<SessionCookie>(envKey, passPhrase);
            builder.Run(this.setFunc);
        }

    }
}

