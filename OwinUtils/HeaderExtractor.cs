namespace OwinUtils
{
    using System;
    using System.Threading.Tasks;
    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>; 
    using EnvDict = System.Collections.Generic.IDictionary<string, object>;
    using HeaderDict = System.Collections.Generic.IDictionary<string, string[]>;
    using Owin;
    using Microsoft.Owin;

    public class HeaderExtractor
    {
        AppFunc next;
        string header;

        public HeaderExtractor(AppFunc next, string header)
        {
            this.next = next;
            this.header = header;
        }

        public Task Invoke(EnvDict env)
        {
            var ctx = new OwinContext(env);
            env["RouteParams"] = ctx.Request.Headers.Get(this.header);
            return next.Invoke(env);
        }

    }
}

