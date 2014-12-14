using Microsoft.Owin;
using Owin;
using System;
using System.Linq;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;
using System.IO;

namespace OwinUtils
{
    /// <summary>
    /// Extension methods for IAppBuilder which can be used to define Routes.
    /// Extracting values from inbound requests (Cookies, Header, Body, URL segments)
    /// and passing them to Route functions matching the inbound values to parameters
    /// in the route function delegates
    /// </summary>
    public static class RouteBuilder
    {

        /// <summary>
        /// Extracts a named query parameter from the inbound request URL
        /// and adds it to the RouteParams for use in a Route further down the middleware chain
        /// 
        /// <example>
        /// Use as:
        /// <code>
        ///     // declare a delegate with parameter names matching the RouteParam keys
        ///     delegate Task RouteFunc(EnvDict env, string myQueryParam);
        /// 
        ///     // create an instance of the delegate. The parameter names don't matter here
        ///     RouteFunc routeFunc = (env, qp) => { // implement route func here }
        /// 
        ///     // Use RouteQuery to extract the query parameters
        ///     builder.Use&lt;RouteQuery&gt;("myQueryParam", "defaultValue")
        /// 
        ///     // Add a route method which will be passed the myQueryParam from the inbound request
        ///     builder.RouteGet(routeFunc, "/")
        /// </code>
        /// </example>
        /// </summary>
        public static IAppBuilder RouteQuery(this IAppBuilder app, string routeParamName, string defaultValue)
        {
            return app.Use<RouteQuery>(routeParamName, defaultValue);
        }

        /// <summary>
        /// Extracts a cookie from the inbound request and adds to the RouteParam collection
        /// (using <paramref name="inRouteParam"/> as the key) making it available for downstream routes
        /// 
        /// On the way back out gets the value from the routeparams (using <paramref name="outRouteParam"/> as the key) 
        /// and returns it to the caller in a cookie called <paramref name="cookieName"/>
        /// </summary>
        public static IAppBuilder RouteCookie(this IAppBuilder iab, string cookieName, string inRouteParam, string outRouteParam)
        {
            return iab.Use<RouteCookie>(cookieName, inRouteParam, outRouteParam);
        }

        /// <summary>
        /// Extracts an HTTP header (<paramref name="headerName"/>) from the inbound request and makes it available
        /// in the RouteParams as <paramref name="routeParamKey"/>
        /// </summary>
        /// <param name="headerName">Name of the HTTP header to inject</param>
        /// <param name="routeParamKey">Key to use for the result in RouteParams</param>
        public static IAppBuilder RouteHeader(this IAppBuilder iab, string headerName, string routeParamKey)
        {
            return iab.Use<RouteHeader>(headerName, routeParamKey);
        }

        /// <summary>
        /// Creates a branch in the pipeline. If the inbound request matches "template"
        /// then the midlewares defined by <paramref name="branchBuilder"/> will be called. Any parameters
        /// extracted by the template will be added to RouteParams for use within the branch.
        /// The BasePath and Path variables in the owin Environment will be adjusted and then restored
        /// when the branch is complete
        /// 
        /// If the template is not matched this does nothing and the middleware after this branch will be called
        /// </summary>
        /// <param name="template">A string defining the RouteTemplate to match.</param>
        /// <param name="branchBuilder">An Action which adds middleware to this branch</param>
        public static IAppBuilder Branch(this IAppBuilder app, string template, Action<IAppBuilder> branchBuilder)
        {
            var rt = new RouteTemplate(template, true);
            return Branch(app, rt, branchBuilder);
        }

        /// <summary>
        /// If the request Path matches <paramref name="template"/> and the <paramref name="httpMethod"/> is matched then
        /// Any components matched in the template are added to the RouteParams
        /// and the middleware function <paramref name="routeAction"/> is Invoked
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
        /// If template and httpMethod are matched by the inbound request then delegate <paramref name="callee"/> will
        /// be invoked. RouteParams will be extracted from <paramref name="template"/> and  
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

        /// <summary>
        /// Variant of the Microsoft.Owin method IAppbuilder.Run which supports a plain middleware Func
        /// rather than an IOwinMiddleware
        /// </summary>
        /// <param name="runAction">The middleware to run</param>
        public static void Run(this IAppBuilder app, AppFunc runAction)
        {
            app.Run(ctx => runAction(ctx.Environment));
        }


        /// <summary>
        /// The most general for of Route to call a method on an object when the route is matched
        /// </summary>
        /// <param name="app">The IAppBuilder instance</param>
        /// <param name="httpMethod">The HTTP method name to match</param>
        /// <param name="callee">An object implmenting <paramref name="methodName"/> which will be called when the route is matched</param>
        /// <param name="methodName">The method to call on <paramref name="callee"/></param>
        /// <param name="template">A string defining the template</param>
        public static IAppBuilder Route(this IAppBuilder app, string httpMethod, object callee, string methodName, string template)
        {
            var rt = new[]{ new RouteTemplate(template, false)} ;
            return Route(app, httpMethod, callee, methodName, rt);
        }

       
        /// <summary>
        /// Extracts the body of the request injects it into the routeparams to be used downstream
        /// if the httpMethod of the request is one of the ones in httpMethods
        /// </summary>
        /// <param name="httpMethods">Http methods to be matched  (eg. {"PUT", "POST"} )</param>
        /// <param name="routeParamKey">Key in the RouteParams which will hold the output of converter</param>
        /// <param name="converter">Function which will be passed the body of the request and can convert it.</param>
        public static IAppBuilder RouteBody(this IAppBuilder iab, string[] httpMethods, string routeParamKey, Func<Stream, object> converter)
        {
            return iab.Use<RouteBody>(httpMethods, routeParamKey, converter);
        }

        /// <summary>
        /// Extracts the body of the request injects it into the routeparams to be used downstream
        /// if the httpMethod of the request is one of the ones in httpMethods
        /// </summary>
        /// <param name="httpMethods">Http methods.</param>
        /// <param name="routeParamKey">Parameter key.</param>
        /// <param name="converter">Converter.</param>
        public static IAppBuilder RouteBody(this IAppBuilder iab, string httpMethod, string routeParamKey, Func<Stream, object> converter)
        {
            return iab.RouteBody(new[] {httpMethod}, routeParamKey, converter);
        }

        /// <summary>
        /// Extracts the body of the request injects it into the routeparams to be used downstream
        /// if the httpMethod of the request is one of the ones in httpMethods
        /// </summary>
        /// <param name="httpMethod">Http methods to match</param>
        /// <param name="routeParamKey">Parameter key.</param>
        /// <param name="converter">Converter.</param>
        public static IAppBuilder RouteBody(this IAppBuilder iab, string httpMethod, string routeParamKey)
        {
            return iab.RouteBody(new[] { httpMethod }, routeParamKey, null);
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

