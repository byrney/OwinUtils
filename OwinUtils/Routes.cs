using System;
using Owin;
using Microsoft.Owin;
using System.Threading.Tasks;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;
using RouteFunc = System.Func<System.Collections.Generic.IDictionary<string, object>
    , System.Collections.Generic.IDictionary<string, object>
, System.Threading.Tasks.Task>; 

namespace ConsoleHost
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
                if (!String.IsNullOrEmpty(seg)) {
                    Parse(seg);
                }
                else {
                    this.type = TokenType.literal;
                    this.name = "";
                }
            }

            private void Parse(string seg)
            {
                var first = seg[0];
                switch (first) {
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

            public bool extract(string segment, out string value)
            {
                value = null;
                switch (this.type) {
                    case TokenType.literal:
                        return segment == name;
                        break;
                    case TokenType.optional:
                        value = segment;
                        return true;
                        break;
                    case TokenType.required:
                        value = segment;
                        return !String.IsNullOrEmpty(segment);
                        break;
                }
                return false;
            }

        }

        private Token[] tokens;
        public RouteTemplate(string template)
        {
            var segs = template.Split('/');
            this.tokens = new Token[segs.Length];
            for (int i = 0; i < segs.Length; i++) {
                this.tokens[i] = new Token(segs[i]);
            }
        }

        public bool match(PathString path)
        {
            var segs = path.Value.Split('/');
            for (int i = 0; i < this.tokens.Length; i++) {
                string value;
                var seg = i < segs.Length ? segs[i] : null;
                if (!tokens[i].extract(seg, out value)) {
                    return false;
                }
            }
            return true;
        
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
        public static IAppBuilder Branch(this IAppBuilder app, string template, Action<IAppBuilder> action)
        {
            var rt = new RouteTemplate(template);
            return Branch(app, rt, action);
        }

        // converts the string to a Template and calls the corresponding overload
        public static IAppBuilder Route(this IAppBuilder app, string template, AppFunc action)
        {
            var rt = new RouteTemplate(template);
            return Route(app, rt, action);
        }

        // creates a branch in the routing
        public static IAppBuilder Branch(this IAppBuilder app, RouteTemplate template, Action<IAppBuilder> branchAction)
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

        public static void Run(this IAppBuilder app, AppFunc runAction)
        {
            app.Run(ctx => runAction(ctx.Environment));
        }

    }

}

