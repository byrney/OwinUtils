namespace OwinUtils
{
    using System;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>; 
    using EnvDict = System.Collections.Generic.IDictionary<string, object>;
    using HeaderDict = System.Collections.Generic.IDictionary<string, string[]>;

    public class Forwarded
    {
    
        AppFunc downstream;
        string localRoot;
        string environmentKey = "forwarded";

        public Forwarded(AppFunc next)
            :this(next, "/")
        {
        }

        public Forwarded(AppFunc next, string localRoot)
        {
            if (next == null) {
                throw new InvalidOperationException("Next middleware cannot be null");
            }
            this.downstream = next;
            this.localRoot = localRoot;
        }

        public static string GetHeaderOrNull(IDictionary<string, string[]> dictionary, string key)
        {
            string[] ret = null;
            // Ignore return value
            dictionary.TryGetValue(key, out ret);
            return ret == null ? null : ret[0];
        }

        public Task Invoke(EnvDict env)
        {
            var headers =  (HeaderDict)env["owin.RequestHeaders"];
            var fdict = new Dictionary<string, object>();
            var context = new Microsoft.Owin.OwinContext(env);
            if(headers.ContainsKey("X-Forwarded-For")) {
                fdict.Add("ScriptName", GetHeaderOrNull(headers, "X-Script-Name"));
                fdict.Add("Host",       GetHeaderOrNull(headers, "X-Forwarded-Host"));
                fdict.Add("Scheme",     GetHeaderOrNull(headers, "X-Forwarded-Proto"));
            }else{
                fdict.Add("ScriptName", this.localRoot);
                fdict.Add("Host", context.Request.Host.ToString());
                fdict.Add("Scheme", context.Request.Scheme.ToString());
            }
            fdict.Add("LocalRoot", this.localRoot);
            env.Add(this.environmentKey, fdict);
            return this.downstream.Invoke(env);
        }

    }
}

