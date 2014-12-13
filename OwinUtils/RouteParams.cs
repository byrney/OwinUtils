using System.Net;
using Microsoft.Owin;
using System;
using System.Threading.Tasks;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;
using RouteDict = System.Collections.Generic.Dictionary<string, object>;
using EnvDict = System.Collections.Generic.IDictionary<string, object>;

namespace OwinUtils
{

    public static class RouteParams
    {
        private const string routeKey = "RouteParams";

        public static void Set(EnvDict env, string key, object v)
        {
            if (v == null) {
                return;
            }
            var routeDict = InitDictionary(env);
            routeDict[key] = v;
        }

        public static RouteDict Merge(EnvDict env, RouteDict toMerge)
        {
            if (toMerge == null) {
                return null;
            }
            var existing = InitDictionary(env);
            foreach (var item in toMerge)
            {
                existing[item.Key] = item.Value;
            }
            return existing;
        }

        public static RouteDict GetDict(EnvDict env)
        {
            object routeDict;
            if (env.TryGetValue(routeKey, out routeDict)) {
                return (RouteDict) routeDict;
            }
            return null;
        }

        public static T GetParam<T>(EnvDict env, string key)
        {
            object routeDict;
            if (env.TryGetValue(routeKey, out routeDict)) {
                var rd = (RouteDict) routeDict;
                object v;
                if (rd.TryGetValue(key, out v)) {
                    return (T) v;
                }
            }
            return default(T);
        }

        public static RouteDict CreateDict()
        {
            return new System.Collections.Generic.Dictionary<string, object>();
        }

        private static RouteDict InitDictionary(EnvDict env)
        {
            object routeDict;
            if (!env.TryGetValue(routeKey, out routeDict)) {
                routeDict = CreateDict();
                env[routeKey] = routeDict;
            }
            return (RouteDict) routeDict;
        }
    }
 
}
