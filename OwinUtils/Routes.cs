using System;
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

        public bool match(PathString path, RouteDict paramDict)
        {
            var segs = path.Value.Split('/');
            for (int i = 0; i < this.tokens.Length; i++)
            {
                string value;
                var seg = i < segs.Length ? segs[i] : null;
                if (!tokens[i].extract(seg, paramDict))
                {
                    return false;
                }
            }
            return true;

        }
    }

    public class RouteMiddleware : OwinMiddleware
    {
        public const string RouteParamsKey = "RouteParams";
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
            var routeParams = new System.Collections.Generic.Dictionary<string, object>();
            if (options.template.match(ctx.Request.Path, routeParams))
            {
                var env = ctx.Environment;
                env[RouteParamsKey] = routeParams;  //todo: merge dicts
                return options.app != null ? options.app.Invoke(env) : options.branch.Invoke(ctx);
            }
            else
            {
                return Next.Invoke(ctx);
            }
        }

    }

 
}

