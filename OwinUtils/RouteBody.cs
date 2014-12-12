using System.IO;
using System.Linq;

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
    /// Extracts the body from the inbound request
    /// and adds it to the RouteParams for use in a Route further down the middleware chain
    /// The Type of the object added to the routeparams is Stream by default
    /// 
    /// 
    /// </summary>
    class RouteBody
    {
        readonly AppFunc next;
        readonly string routeParamName;
        private readonly string[] methods;
        private readonly Func<Stream, object> converter;

        public RouteBody(AppFunc next, string[] httpMethods, string paramKey, Func<Stream, object> converter)
        {
            this.next = next;
            this.routeParamName = paramKey;
            this.methods = httpMethods;
            this.converter = converter;
        }

        public Task Invoke(EnvDict env)
        {
            var ctx = new OwinContext(env);
            if (this.methods.Contains(ctx.Request.Method)) {
                var bodyStream = ctx.Request.Body;
                var v = this.converter != null ? this.converter(bodyStream) : bodyStream;
                if (v != null)
                {
                    RouteParams.Set(env, this.routeParamName, v);
                }
            }
            return next.Invoke(env);
        }
    }


    public static class AppBuilderRouteBodyExtensions
    {
        // Extracts a query parameters and injects it into the routeparams to be used downstream
        // if the httpMethod of the request is one of the ones in httpMethods
        public static IAppBuilder RouteBody(this IAppBuilder iab, string[] httpMethods, string paramKey, Func<Stream, object> converter)
        {
            return iab.Use<RouteBody>(httpMethods, paramKey, converter);
        }

        // Extracts a query parameters and injects it into the routeparams to be used downstream
        // the result of passing the body Stream through converter is added to the routeparams
        public static IAppBuilder RouteBody(this IAppBuilder iab, string httpMethod, string paramKey, Func<Stream, object> converter)
        {
            return iab.RouteBody(new[] {httpMethod}, paramKey, converter);
        }

        // Extracts a query parameters and injects it into the routeparams to be used downstream
        // the body is passed as a Stream in the routeparams
        public static IAppBuilder RouteBody(this IAppBuilder iab, string httpMethod, string paramKey)
        {
            return iab.RouteBody(new[] { httpMethod }, paramKey, null);
        }

    }

}

