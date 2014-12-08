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
    public class RouteBody
    {
        AppFunc next;
        string routeParamName;
        private string defaultValue;
        private string[] methods;
        private Func<Stream, object> converter;

        public RouteBody(AppFunc next, string httpMethod, Func<Stream, object> converter, string paramKey)
        :this(next, new[] {httpMethod}, converter, paramKey)
        {
        }

        public RouteBody(AppFunc next, string[] httpMethods, Func<Stream, object> converter, string paramKey)
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
                var v = this.converter(bodyStream);
                if (v != null)
                {
                    RouteParams.Set(env, this.routeParamName, v);
                }
            }
            return next.Invoke(env);
        }

    }
}

