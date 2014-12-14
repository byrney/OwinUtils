using System;
using System.Threading.Tasks;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>; 
using EnvDict = System.Collections.Generic.IDictionary<string, object>;
using HeaderDict = System.Collections.Generic.IDictionary<string, string[]>;
using Owin;
using Microsoft.Owin;


namespace OwinUtils
{
    class RouteHeader
    {
        AppFunc next;
        string header;
        private string paramKey;

     

        public RouteHeader(AppFunc next, string header, string routeParamKey)
        {
            this.next = next;
            this.header = header;
            this.paramKey = routeParamKey;
        }

        public Task Invoke(EnvDict env)
        {
            var ctx = new OwinContext(env);
            var v = ctx.Request.Headers.Get(this.header);
            RouteParams.Set(env, this.paramKey, v);
            return next.Invoke(env);
        }

    }



}

