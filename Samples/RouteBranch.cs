using System;
using OwinUtils;
using Owin;
using Microsoft.Owin;
using EnvDict = System.Collections.Generic.IDictionary<string, object>;
using System.Threading.Tasks;

namespace OwinUtils.Samples
{
    public class RouteBranchSample
    {
        public delegate Task WriteSegmentFunc(IOwinContext ctx, string segment);

        public static Task WriteToBody(IOwinContext ctx, string msg)
        {
            var p = string.Format("\nPath:\t\t'{0}'\nBasePath:\t'{1}'\nSegment:\t{2}\n", ctx.Request.Path, ctx.Request.PathBase, msg);
            return ctx.Response.WriteAsync(p );
        }

        private static WriteSegmentFunc branch1 = (ctx, w) => WriteToBody(ctx, "Branch 1: " + w);
        private static WriteSegmentFunc branch2 = (ctx, w) => WriteToBody(ctx, "Branch 2: " + w);

        public static void BuildSample(IAppBuilder app)
        {
            // branch 1 extracting a URL segment
            app.Branch("/first/<segment>", b => {
                // route with no extract. Gets params from branch
                b.RouteGet(branch1, "/", "");
            });
            // branch 2 does not extract
            app.Branch("/second/<segment>", b => {
                // the route in branch2 extracts
                b.RouteGet(branch2, "/<segment>", "");
            });
            // Wont match a branch
            app.Run(ctx => WriteToBody(ctx, "NONE"));
        }

    }
}
