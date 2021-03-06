﻿using System.Net;
using Microsoft.Owin;
using System;
using System.Threading.Tasks;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;
using RouteDict = System.Collections.Generic.Dictionary<string, object>;
using EnvDict = System.Collections.Generic.IDictionary<string, object>;

namespace OwinUtils
{


    class RouteMiddleware : OwinMiddleware
    {
        
        public class Options
        {
            public Options(string httpMethod, RouteTemplate template, AppFunc app)
                : this(httpMethod, new[] { template}, app)
            {
            }

            public Options(string httpMethod, RouteTemplate[] templates, AppFunc app)
            {
                this.httpMethod = httpMethod;
                this.templates = templates;
                this.app = app;
            }

            public string httpMethod = null;
            public OwinMiddleware branch { get; set;}
            public RouteTemplate[] templates { get ; set; }
            public AppFunc app { get ; set; }
        }

        private Options options;
        public RouteMiddleware(OwinMiddleware next, Options options)
            :base(next)
        {
            this.options = options;
        }

        private RouteTemplate.MatchData MatchMethodAndTemplate(IOwinContext ctx, string path)
        {
            var requestMethod = ctx.Request.Method;
            if (options.httpMethod != null && options.httpMethod != requestMethod) {
                return null;
            }
            foreach (var template in options.templates) {
                var match = template.match(path);
                if (match != null) {
                    return match;
                }
            }
            return null;
        }

        public new string ToString()
        {
            return options.httpMethod ?? "ALL" + options.templates[0].ToString();
        }

        public override Task Invoke(IOwinContext ctx)
        {
            var requestMethod = ctx.Request.Method;
            var routeParams = new System.Collections.Generic.Dictionary<string, object>();
            string remainder;
            var path = ctx.Request.Path.Value;
            var match = MatchMethodAndTemplate(ctx, path);
            if (match == null)
            {
                return Next.Invoke(ctx);
            }
            var env = ctx.Environment;
            RouteParams.Merge(env, match.extracted);
            var oldBase = ctx.Request.PathBase;
            var oldPath = ctx.Request.Path;
            ctx.Request.PathBase = new PathString(oldBase + match.pathMatched);
            ctx.Request.Path = new PathString(match.pathRemaining);
            var restore = new Action<Task>(task => restorePaths(ctx, oldBase, oldPath));
            if (options.app != null)
            {
                return options.app.Invoke(env).ContinueWith(restore, TaskContinuationOptions.ExecuteSynchronously);
            }
            else
            {
                return options.branch.Invoke(ctx).ContinueWith(restore, TaskContinuationOptions.ExecuteSynchronously);
            }
        }

        private void restorePaths(IOwinContext ctx, PathString oldBase, PathString oldPath)
        {
            ctx.Request.PathBase = oldBase;
            ctx.Request.Path = oldPath;
        }

    }

 
}

