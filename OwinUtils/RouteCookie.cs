using Owin;
using Microsoft.Owin;
using System.Threading.Tasks;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;
using EnvDict = System.Collections.Generic.IDictionary<string, object>;

namespace OwinUtils
{

    class RouteCookie
    {
        readonly string _cookieName;
        private readonly CookieConverter converter;
        private readonly string inRouteParam;
        private readonly string outRouteParam;

        public RouteCookie(AppFunc next, string cookieName, string inRouteParam, string outRouteParam)
        {
            this.converter = new CookieConverter(next, cookieName, Inbound, Outbound);
            this._cookieName = cookieName;
            this.inRouteParam = inRouteParam;
            this.outRouteParam = outRouteParam;
        }

        private void Inbound(IOwinRequest request, string beforeValue)
        {
            if (beforeValue != null)
            {
                RouteParams.Set(request.Environment, inRouteParam, beforeValue);
            }
        }

        private string Outbound(IOwinResponse response)
        {
            return RouteParams.GetParam<string>(response.Environment, outRouteParam);
        }

        public Task Invoke(EnvDict env)
        {
            return converter.Invoke(env);
        }

    }




}

