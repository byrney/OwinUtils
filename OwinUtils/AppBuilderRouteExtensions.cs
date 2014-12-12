using Microsoft.Owin;
using Owin;
using System;
using System.Linq;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

namespace OwinUtils
{
    public static class AppBuilderRouteExtensions
    {
        // creates a route which calls an AppFunc
        private static IAppBuilder Route(this IAppBuilder app, string httpMethod, AppFunc runAction, RouteTemplate[] templates)
        {
            var options = new RouteMiddleware.Options(httpMethod, templates, runAction);
            IAppBuilder result = app.Use<RouteMiddleware>(options);
            return result;
        }

        // converts the string to a Template and calls the corresponding overload
        public static IAppBuilder Branch(this IAppBuilder app, string template, Action<IAppBuilder> action)
        {
            var rt = new RouteTemplate(template, true);
            return Branch(app, rt, action);
        }

        // converts the string to a Template and calls the corresponding overload
        public static IAppBuilder Route(this IAppBuilder app, string template, AppFunc action, string httpMethod = null)
        {
            var rt = new[] { new RouteTemplate(template, false) };
            return Route(app, httpMethod, action, rt);
        }

        // creates a branch in the routing
        private static IAppBuilder Branch(this IAppBuilder app, RouteTemplate template, Action<IAppBuilder> branchAction)
        {
            var options = new RouteMiddleware.Options(null, template, null);
            IAppBuilder result = app.Use<RouteMiddleware>(options);
            IAppBuilder branch = app.New();
            branchAction(branch);
            options.branch = (OwinMiddleware)branch.Build(typeof(OwinMiddleware));
            return result;
        }



     
        // Creates a route which calls methodName on instance callee converting any
        // matching entries in env["routeParams"] to arguments of callee.methodName
        private static IAppBuilder Route(this IAppBuilder app, string httpMethod, object callee, string methodName, RouteTemplate[] templates)
        {
            if (callee == null) {
                var msg = string.Format("Null target for route {0} {1}", httpMethod, templates[0]);
                throw new ArgumentNullException("callee", msg);
            }
            var wrapper = new Wrapper(callee, methodName);
            return Route(app, httpMethod, wrapper.Invoke, templates);
        }

        // Creates a route which calls methodName on instance callee converting any
        // matching entries in env["routeParams"] to arguments of callee.methodName
        public static IAppBuilder Route(this IAppBuilder app, string httpMethod, object callee, string methodName, string template)
        {
            var rt = new[]{ new RouteTemplate(template, false)} ;
            return Route(app, httpMethod, callee, methodName, rt);
        }

        // Creates a route which calls methodName on instance callee converting any
        // matching entries in env["routeParams"] to arguments of callee.methodName
        private static IAppBuilder Route(this IAppBuilder app, string httpMethod, object callee, string methodName, string[] templates)
        {
            RouteTemplate[] rt = templates.Select(t => new RouteTemplate(t, false)).ToArray();
            return Route(app, httpMethod, callee, methodName, rt);
        }

        public static IAppBuilder Route(this IAppBuilder app, string httpMethod, Delegate callee, string template)
        {
            var rt = new[] { new RouteTemplate(template, false) };
            return Route(app, httpMethod, callee, "Invoke", rt);
        }

        public static IAppBuilder RouteGet(this IAppBuilder app, Delegate callee, params string[] templates)
        {
            return Route(app, "GET", callee, "Invoke", templates);
        }

        public static IAppBuilder RoutePost(this IAppBuilder app, Delegate callee, params string[] templates)
        {
            return Route(app, "POST", callee, "Invoke", templates);
        }

        public static IAppBuilder RouteDel(this IAppBuilder app, Delegate callee, params string[] templates)
        {
            return Route(app, "DELETE", callee, "Invoke", templates);
        }


        public static void Run(this IAppBuilder app, AppFunc runAction)
        {
            app.Run(ctx => runAction(ctx.Environment));
        }


    }

}

