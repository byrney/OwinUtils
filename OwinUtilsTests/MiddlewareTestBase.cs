using Microsoft.Owin.Testing;

namespace OwinUtilsTests
{

    using System;
    using System.Net.Http;
    using Owin;

    public class MiddlewareTestBase
    {
        public static HttpResponseMessage testServerGet(Action<IAppBuilder> builder, HttpRequestMessage request, bool headersOnly = false)
        {
            var completionOption = headersOnly ? HttpCompletionOption.ResponseHeadersRead : HttpCompletionOption.ResponseContentRead;
            using (var server = TestServer.Create(builder)) {
                var client = server.HttpClient;
                return client.SendAsync(request, completionOption).Result;
            }
        }

        public static string extractCookie(HttpResponseMessage response, string cookieName)
        {
            var cookieHeader = response.Headers.GetValues("set-cookie");
            foreach (string c in cookieHeader) {
                var decoded = Uri.UnescapeDataString(c);
                if (decoded.StartsWith(cookieName + "=")) {
                    var split = decoded.Split(new char[] { ':', ';' });
                    return split[1];
                }
            }
            return null;
        }

    }
}

