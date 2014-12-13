using Microsoft.Owin;
using Owin;
using System;
using System.Linq;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

namespace OwinUtils
{
    public static class AppBuilderRouteExtensions
    {

        /// <summary>
        /// Creates a branch in the pipeline. If the inbound request matches "template"
        /// then the midlewares defined by branchBuilder will be called. Any parameters
        /// extracted by the template will be added to RouteParams for use within the branch.
        /// The BasePath and Path variables in the owin Environment will be adjusted and then restored
        /// when the branch is complete
        /// 
        /// If the template is not matched this does nothing.
        /// </summary>
        /// <param name="template">A string defining the RouteTeplate to match.</param>
        /// <param name="branchBuilder">An Action which adds middleware to this branch</param>
        public static IAppBuilder Branch(this IAppBuilder app, string template, Action<IAppBuilder> branchBuilder)
        {
            var rt = new RouteTemplate(template, true);
            return Branch(app, rt, branchBuilder);
        }

        // converts the string to a Template and calls the corresponding overload
        /// <summary>
        /// If the request Path matches "template" and the httpMethod is matched then
        /// Any components matched in the template are added to the RouteParams
        /// and the middleware function "action" is Invoked
        /// </summary>
        /// <param name="template">string used to construct a RouteTemplate</param>
        /// <param name="routeAction">The middleware function that will be called</param>
        /// <param name="httpMethod">HTTP method to be matched or all methods if null is passed</param>
        public static IAppBuilder Route(this IAppBuilder app, string template, AppFunc routeAction, string httpMethod = null)
        {
            var rt = new[] { new RouteTemplate(template, false) };
            return Route(app, httpMethod, routeAction, rt);
        }

        /// <summary>
        /// If template and httpMethod are matched by the inbound request then "callee" will
        /// be invoked. RouteParams will be extracted from template and  
        /// Any parameters of callee with names that match RouteParams (including those defined by
        /// other middleware ahead of this) will be passed when callee is Invoked. 
        /// </summary>
        /// <param name="httpMethod">Http method to match (GET, POST, PUT etc)</param>
        /// <param name="callee">The delegate method to be invoked with the arguments populated from RouteParams</param>
        /// <param name="template">Used to construct the template to be matched</param>
        public static IAppBuilder Route(this IAppBuilder app, string httpMethod, Delegate callee, string template)
        {
            var rt = new[] { new RouteTemplate(template, false) };
            return Route(app, httpMethod, callee, "Invoke", rt);
        }

        /// <summary>
        /// Overload for Route which matches a httpMethod of "GET"
        /// </summary>
        /// <param name="callee">The delegate method to be invoked with the arguments populated from RouteParams</param>
        /// <param name="template">Used to construct the template to be matched</param>
        public static IAppBuilder RouteGet(this IAppBuilder app, Delegate callee, params string[] templates)
        {
            return Route(app, "GET", callee, "Invoke", templates);
        }

        /// <summary>
        /// Overload for Route which matches a httpMethod of "POST"
        /// </summary>
        /// <param name="callee">The delegate method to be invoked with the arguments populated from RouteParams</param>
        /// <param name="template">Used to construct the template to be matched</param>
        public static IAppBuilder RoutePost(this IAppBuilder app, Delegate callee, params string[] templates)
        {
            return Route(app, "POST", callee, "Invoke", templates);
        }

        /// <summary>
        /// Overload for Route which matches a httpMethod of "DELETE"
        /// </summary>
        /// <param name="callee">The delegate method to be invoked with the arguments populated from RouteParams</param>
        /// <param name="template">Used to construct the template to be matched</param>
        public static IAppBuilder RouteDel(this IAppBuilder app, Delegate callee, params string[] templates)
        {
            return Route(app, "DELETE", callee, "Invoke", templates);
        }


        public static void Run(this IAppBuilder app, AppFunc runAction)
        {
            app.Run(ctx => runAction(ctx.Environment));
        }


        // Creates a route which calls methodName on instance callee converting any
        // matching entries in env["routeParams"] to arguments of callee.methodName
        public static IAppBuilder Route(this IAppBuilder app, string httpMethod, object callee, string methodName, string template)
        {
            var rt = new[]{ new RouteTemplate(template, false)} ;
            return Route(app, httpMethod, callee, methodName, rt);
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

        // creates a route which calls an AppFunc
        private static IAppBuilder Route(this IAppBuilder app, string httpMethod, AppFunc runAction, RouteTemplate[] templates)
        {
            var options = new RouteMiddleware.Options(httpMethod, templates, runAction);
            IAppBuilder result = app.Use<RouteMiddleware>(options);
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
        private static IAppBuilder Route(this IAppBuilder app, string httpMethod, object callee, string methodName, string[] templates)
        {
            RouteTemplate[] rt = templates.Select(t => new RouteTemplate(t, false)).ToArray();
            return Route(app, httpMethod, callee, methodName, rt);
        }


    }

}

