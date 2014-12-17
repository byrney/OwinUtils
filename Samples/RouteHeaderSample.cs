using System;
using OwinUtils;
using Owin;
using Microsoft.Owin;
using EnvDict = System.Collections.Generic.IDictionary<string, object>;
using System.Threading.Tasks;

namespace OwinUtils.Samples
{
    public class RouteHeaderSample
    {
        public delegate Task WriteHeaderFunc(IOwinContext ctx, string userAgent);

        public static Task WriteUserAgent(IOwinContext ctx, string agent)
        {
            return ctx.Response.WriteAsync("User Agent:" + agent + "\n");
        }

        public static void BuildSample(IAppBuilder app)
        {
            // add eventsource middleware
            app.RouteHeader("User-Agent", "userAgent");
            // pass static member as delegate
            app.RouteGet(new WriteHeaderFunc(WriteUserAgent), "/", "");
        }

    }
}

