using Owin;
using Microsoft.Owin;
using System.Threading.Tasks;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;
using EnvDict = System.Collections.Generic.IDictionary<string, object>;

namespace OwinUtils
{

    public static class AppBuilderRouteCookieExtensions
    {
        /// <summary>
        /// Extracts a cookie from the inbound request and adds to the RouteParam collection
        /// (using inRouteParam as the key) making it available for downstream routes
        /// 
        /// On the way back out gets the value from the routeparams (using outRouteParam as the key) 
        /// and returns it to the caller in a cookie called cookieName
        /// </summary>
        public static IAppBuilder RouteCookie(this IAppBuilder iab, string cookieName, string inRouteParam, string outRouteParam)
        {
            return iab.Use<RouteCookie>(cookieName, inRouteParam, outRouteParam);
        }
    }


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

