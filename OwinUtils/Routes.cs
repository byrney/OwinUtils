using System;
using System.Linq;
using Owin;
using Microsoft.Owin;
using System.Threading.Tasks;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;
using EnvDict = System.Collections.Generic.IDictionary<string, object>;
using RouteDict = System.Collections.Generic.Dictionary<string, object>;
using OwinUtils;

namespace OwinUtils
{

    public class RouteTemplate
    {
        class Token
        {
            enum TokenType { literal, optional, required };
            public string name;
            private TokenType type;
            public Token(string seg)
            {
                if (!String.IsNullOrEmpty(seg))
                {
                    Parse(seg);
                }
                else
                {
                    this.type = TokenType.literal;
                    this.name = "";
                }
            }

            private void Parse(string seg)
            {
                var first = seg[0];
                switch (first)
                {
                    case '[':
                        this.type = TokenType.optional;
                        this.name = seg.Trim('[', ']');
                        break;
                    case '<':
                        this.type = TokenType.required;
                        this.name = seg.Trim('<', '>');
                        break;
                    default:
                        this.type = TokenType.literal;
                        this.name = seg;
                        break;
                }
            }

            public bool extract(string segment, EnvDict paraDict)
            {
                switch (this.type)
                {
                    case TokenType.literal:
                        return segment == this.name;
                        break;
                    case TokenType.optional:
                        paraDict[this.name] = segment;
                        return true;
                        break;
                    case TokenType.required:
                        paraDict[this.name] = segment;
                        return !String.IsNullOrEmpty(segment);
                }
                return false;
            }

        }

        private Token[] tokens;
        public RouteTemplate(string template)
        {
            var segs = template.Split('/');
            this.tokens = new Token[segs.Length];
            for (int i = 0; i < segs.Length; i++)
            {
                this.tokens[i] = new Token(segs[i]);
            }
        }

        public class MatchData
        {
            public string pathMatched;
            public string pathRemaining;
        }

        public MatchData match(PathString path, RouteDict paramDict)
        {
            int numMatched = 0;
            var segs = path.Value.Split('/');
            for (int i = 0; i < this.tokens.Length; i++)
            {
                string value;
                var seg = i < segs.Length ? segs[i] : null;
                if (tokens[i].extract(seg, paramDict)) {
                    numMatched += i < segs.Length ? 1 : 0;
                }
                else {
                    return null;
                }
            }
            var ret = new MatchData();
            ret.pathRemaining = "/" + String.Join("/", segs, numMatched, segs.Length - numMatched);
            ret.pathMatched = String.Join("/", segs, 0, numMatched);
            return ret;
        }
    }

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

        private RouteTemplate.MatchData MatchMethodAndTemplate(IOwinContext ctx, RouteDict routeParams)
        {
            var requestMethod = ctx.Request.Method;
            if (options.httpMethod != null && options.httpMethod != requestMethod) {
                return null;
            }
            return options.template.match(ctx.Request.Path, routeParams);
        }

        public override Task Invoke(IOwinContext ctx)
        {
            var requestMethod = ctx.Request.Method;
            var routeParams = new System.Collections.Generic.Dictionary<string, object>();
            string remainder;
            var match = MatchMethodAndTemplate(ctx, routeParams);
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

