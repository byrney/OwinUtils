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




}

