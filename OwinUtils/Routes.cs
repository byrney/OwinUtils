using Microsoft.Owin;
using System;
using System.Threading.Tasks;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;
using RouteDict = System.Collections.Generic.Dictionary<string, object>;

namespace OwinUtils
{
    public class RouteMiddleware : OwinMiddleware
    {
        public const string RouteParamsKey = "RouteParams";
        public class Options
        {
            public string httpMethod = null;
            public OwinMiddleware branch { get; set;}
            public RouteTemplate template { get ; set; }
            public AppFunc app { get ; set; }
        }

        private Options options;
        public RouteMiddleware(OwinMiddleware next, Options options)
            :base(next)
        {
            this.options = options;
        }

        private RouteTemplate.MatchData MatchMethodAndTemplate(IOwinContext ctx, string path , RouteDict routeParams)
        {
            var requestMethod = ctx.Request.Method;
            if (options.httpMethod != null && options.httpMethod != requestMethod) {
                return null;
            }
            return options.template.match(path, routeParams);
        }

        public override Task Invoke(IOwinContext ctx)
        {
            var requestMethod = ctx.Request.Method;
            var routeParams = new System.Collections.Generic.Dictionary<string, object>();
            string remainder;
            var path = ctx.Request.Path.Value;
            var match = MatchMethodAndTemplate(ctx, path, routeParams);
            if (match != null)
            {
                var env = ctx.Environment;
                env[RouteParamsKey] = routeParams;  //todo: merge dicts
                var oldBase = ctx.Request.PathBase;
                var oldPath = ctx.Request.Path;
                ctx.Request.PathBase = new PathString(oldBase + match.pathMatched);
                ctx.Request.Path = new PathString(match.pathRemaining);
                var restore = new Action<Task>(task => restorePaths(ctx, oldBase, oldPath));
                if (options.app != null) {
                    return options.app.Invoke(env).ContinueWith(restore, TaskContinuationOptions.ExecuteSynchronously);
                }
                else {
                    return options.branch.Invoke(ctx).ContinueWith(restore, TaskContinuationOptions.ExecuteSynchronously);
                }
            }
            else
            {
                return Next.Invoke(ctx);
            }
        }

        private void restorePaths(IOwinContext ctx, PathString oldBase, PathString oldPath)
        {
            ctx.Request.PathBase = oldBase;
            ctx.Request.Path = oldPath;
        }

    }

 
}

