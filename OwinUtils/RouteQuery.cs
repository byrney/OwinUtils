namespace OwinUtils
{
    using System;
    using System.Threading.Tasks;
    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;
    using EnvDict = System.Collections.Generic.IDictionary<string, object>;
    using RouteDict = System.Collections.Generic.IDictionary<string, object>;
    using Owin;
    using Microsoft.Owin;

    /// <summary>
    /// Extracts a named query parameter from the inbound request URL
    /// and adds it to the RouteParams for use in a Route further down the middleware chain
    /// 
    /// Use as:
    ///     delegate Task DelFunc(EnvDict env, string myQueryParam);
    /// 
    ///     DelFunc f = (env, qp) => { // implement route func here }
    ///     builder.Use&lt;RouteQuery&gt;("myQueryParam", "defaultValue")
    ///     builder.RouteGet(f, "/")
    /// 
    /// </summary>
    class RouteQuery
    {
        AppFunc next;
        string routeParamName;
        private string defaultValue;

        public RouteQuery(AppFunc next, string routeParamName, string defaultValue)
        {
            this.next = next;
            this.routeParamName = routeParamName;
            this.defaultValue = defaultValue;
        }

        public Task Invoke(EnvDict env)
        {
            var ctx = new OwinContext(env);
            var v = ctx.Request.Query.Get(this.routeParamName) ?? this.defaultValue;
            if (v != null) {
                RouteParams.Set(env, this.routeParamName, v);
            }
            return next.Invoke(env);
        }
    }

    public static class AppBuilderRouteQueryExtensions
    {
        // Extracts a query parameters and injects it into the routeparams to be used downstream
        public static IAppBuilder RouteQuery(this IAppBuilder app, string routeParamName, string defaultValue)
        {
            return app.Use<RouteQuery>(routeParamName, defaultValue);
        }
    }

}

