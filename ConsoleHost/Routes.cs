using System;
using Owin;
using Microsoft.Owin;
using System.Threading.Tasks;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>; 

namespace ConsoleHost
{

    public class RouteTemplate
    {
        class Segment 
        {
            enum SegmentType { literal, optional, required };
            public string name;
            private SegmentType type;
            public Segment(string seg)
            {
                // todo:
                this.name = seg;
                this.type = SegmentType.literal;
            }
        }

        private Segment[] segments;
        public RouteTemplate(string template)
        {
            var segs = template.Split('/');
            this.segments = new Segment[segs.Length];
            for (int i = 0; i < segs.Length; i++) {
                this.segments[i] = new Segment(segs[i]);
            }
        }

        public bool match(PathString path)
        {
            // todo:
            return path.StartsWithSegments(new PathString(segments[0].name));
        }
    }

    public class RouteMiddleware : OwinMiddleware
    {
        public class Options {
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

        public override Task Invoke(IOwinContext ctx)
        {
            if (options.template.match(ctx.Request.Path)) {
                var env = ctx.Environment;
                return options.app != null ? options.app.Invoke(env) : options.branch.Invoke(ctx);
            } else {
                return Next.Invoke(ctx);
            }
        }
    }

    public static class RouteBuilder
    {
        // converts the string to a Template and calls the corresponding overload
        public static IAppBuilder Route(this IAppBuilder app, string template, Action<IAppBuilder> action)
        {
            var rt = new RouteTemplate(template);
            return Route(app, rt, action);
        }

        // converts the string to a Template and calls the corresponding overload
        public static IAppBuilder Route(this IAppBuilder app, string template, AppFunc action)
        {
            var rt = new RouteTemplate(template);
            return Route(app, rt, action);
        }

        // creates a branch in the routing
        public static IAppBuilder Route(this IAppBuilder app, RouteTemplate template, Action<IAppBuilder> branchAction)
        {
            var options = new RouteMiddleware.Options();
            options.template = template;
            IAppBuilder result = app.Use<RouteMiddleware>(options);
            IAppBuilder branch = app.New();
            branchAction(branch);
            options.branch = (OwinMiddleware)branch.Build(typeof(OwinMiddleware));
            return result;
        }

        // creates a route which calls runAction
        public static IAppBuilder Route(this IAppBuilder app, RouteTemplate template, AppFunc runAction)
        {
            var options = new RouteMiddleware.Options();
            options.template = template;
            options.app = runAction;
            IAppBuilder result = app.Use<RouteMiddleware>(options);
            return result;
        }


    }

}

