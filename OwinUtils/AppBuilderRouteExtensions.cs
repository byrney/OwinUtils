using System;
using Owin;
using Microsoft.Owin;
using System.Threading.Tasks;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>; 
using OwinUtils;

namespace OwinUtils
{
    public static class AppBuildRouteExtensions
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

        // creates a route which calls an AppFunc
        public static IAppBuilder Route(this IAppBuilder app, RouteTemplate template, AppFunc runAction)
        {
            var options = new RouteMiddleware.Options();
            options.template = template;
            options.app = runAction;
            IAppBuilder result = app.Use<RouteMiddleware>(options);
            return result;
        }

        // Creates a route which calls methodName on instance callee converting any
        // matching entries in env["routeParams"] to arguments of callee.methodName
        public static IAppBuilder Route(this IAppBuilder app, RouteTemplate template, object callee, string methodName)
        {
            var wrapper = new Wrapper(callee, methodName);
            return Route(app, template, wrapper.Invoke);
        }

        // Creates a route which calls methodName on instance callee converting any
        // matching entries in env["routeParams"] to arguments of callee.methodName
        public static IAppBuilder Route(this IAppBuilder app, string template, object callee, string methodName)
        {
            var rt = new RouteTemplate(template);
            return Route(app, rt, callee, methodName);
        }

        public static void Run(this IAppBuilder app, AppFunc runAction)
        {
            app.Run(ctx => runAction(ctx.Environment));
        }

    }

}

