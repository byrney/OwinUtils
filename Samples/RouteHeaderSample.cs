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
        public delegate RouteReturn WriteHeaderFunc(IOwinContext ctx, string userAgent);

        public static RouteReturn WriteUserAgent(IOwinContext ctx, string agent)
        {
            return new RouteReturn {
                StringBody = "User Agent:" + agent + "\n"
            };
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

