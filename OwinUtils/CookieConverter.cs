using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace OwinUtils
{
    /// <summary>
    /// Extracts a named cookie from the inbound request and passes it to
    /// a Func inbound which should inject the cookie value into the request environment
    /// This is reversed on the way back out. The outbound Func extracts the cookie value
    /// from the environment and then this middleware converts it to a cookie
    /// </summary>
    internal class CookieConverter
    {
        private readonly Func<IDictionary<string, object>, Task> nextMiddleware;
        private readonly Action<IOwinRequest, string> inboundFunc;
        private readonly Func<IOwinResponse, string> outboundFunc;
        private string cookieName;

        public CookieConverter(Func<IDictionary<string, object>, Task> next, string cookieName
                                                            , Action<IOwinRequest, string> inbound
                                                            , Func<IOwinResponse, string> outbound)
        {
            this.nextMiddleware = next;
            this.cookieName = cookieName;
            this.inboundFunc = inbound;
            this.outboundFunc = outbound;
        }

        public Task Invoke(IDictionary<string, object> env)
        {
            return this.Invoke(new OwinContext(env));
        }

        public Task Invoke(IOwinContext context)
        {
            var request = context.Request;
            if (context.Request.Headers["cookie"] != null)
            {
                string inboundValue = request.Cookies[this.cookieName];
                if (inboundValue != null)
                {
                    inboundFunc(request, inboundValue);
                }
            }
            context.Response.OnSendingHeaders(state =>
            {
                var resp = (IOwinResponse)state;
                convertToCookie(resp);
            }, context.Response);
            return this.nextMiddleware.Invoke(context.Environment);
        }

        private void convertToCookie(IOwinResponse response)
        {
            var outboundValue = outboundFunc(response);
            if (outboundValue != null)
            {
                // todo:  only set cookie if different from outbound
                response.Cookies.Append(this.cookieName, outboundValue);
            }
        }
   
    }
}