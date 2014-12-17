using System;
using OwinUtils;
using Owin;
using Microsoft.Owin;
using EnvDict = System.Collections.Generic.IDictionary<string, object>;
using System.Threading.Tasks;

namespace OwinUtils.Samples
{
    public class RouteParamsSample
    {

        public static Task WriteUserAgent(IOwinContext ctx)
        {
            // manually get the value from the route params dictionary
            string agent = RouteParams.GetParam<string>(ctx.Environment, "userAgent");
            return ctx.Response.WriteAsync("User Agent:" + agent + "\n");
        }

        public static void BuildSample(IAppBuilder app)
        {
            // extract user agent header
            app.RouteHeader("User-Agent", "userAgent");
            app.Run(ctx => WriteUserAgent(ctx));
        }

    }
}

