using System;
using System.Threading.Tasks;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;
using EnvDict = System.Collections.Generic.IDictionary<string, object>;
using RouteDict = System.Collections.Generic.IDictionary<string, object>;
using Owin;
using Microsoft.Owin;

namespace OwinUtils
{



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




}

