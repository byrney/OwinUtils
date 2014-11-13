namespace OwinUtils
{
    using System;
    using System.Threading.Tasks;
    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;
    using EnvDict = System.Collections.Generic.IDictionary<string, object>;
    using RouteDict = System.Collections.Generic.IDictionary<string, object>;
    using Owin;
    using Microsoft.Owin;

    public class QueryExtractor
    {
        AppFunc next;
        string key;
        private string defaultValue;

        public QueryExtractor(AppFunc next, string key, string defaultValue)
        {
            this.next = next;
            this.key = key;
            this.defaultValue = defaultValue;
        }

        public Task Invoke(EnvDict env)
        {
            var ctx = new OwinContext(env);
            var v = ctx.Request.Query.Get(this.key) ?? this.defaultValue;
            if (v != null) {
                RouteParams.Set(env, this.key, v);
            }
            return next.Invoke(env);
        }

    }
}

