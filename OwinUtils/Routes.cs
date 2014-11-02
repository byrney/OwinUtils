using System;
using Owin;
using Microsoft.Owin;
using System.Threading.Tasks;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>; 
using OwinUtils;

namespace OwinUtils
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
            return path.StartsWithSegments(new PathString("/" + segments[0].name));
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

 
}

