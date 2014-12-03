using Microsoft.Owin;
using Microsoft.Owin.Testing;
using NUnit.Framework;
using Owin;
using OwinUtils;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using EnvDict = System.Collections.Generic.IDictionary<string, object>;


namespace OwinUtilsTests
{
    class RouteCookieTests
    {
        private string _extractedUrl;
        private string _extractedCookie;
        private string _defaultCookieValue;

        public RouteCookieTests()
        {
            _extractedUrl = null;
            _extractedCookie = null;
            _defaultCookieValue = "berry default";
        }

        [Test]
        public void BasicRoute()
        {
        }

        private delegate Task TestFunc(EnvDict env, string fromUrl, string fromCookie);

        [Test]
        public void RouteDictIsPassedCorrectly()
        {
            // given: a server that updates member variables
            // with the values passed and a default value
            var ts = CreateTestServer();

            // WHen: A request from the client is sent which includes
            // the cookie
            string expectedFromUrl = "peaches";
            string expectedFromCookie = "apples";
            var cl = ts.HttpClient;
            var url = String.Format("http://example.com/root/{0}", expectedFromUrl);
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Cookie", string.Format("fromCookie={0}", expectedFromCookie));
            var resp = cl.SendAsync(request).Result;

            // Then: values are available in the local varaiables
            Assert.IsTrue(resp.IsSuccessStatusCode);
            Assert.AreEqual(expectedFromUrl, _extractedUrl);
            Assert.AreEqual(expectedFromCookie, _extractedCookie);
        }

        [Test]
        public void NonMatchingCookiesAreIgnored()
        {
            // given: a server that updates member variables
            _defaultCookieValue = "creamy dreamy";
            var ts = CreateTestServer();

            // WHen: A request from the client is sent with cookies
            // that don't match the cookie name
            string expectedFromUrl = "peaches";
            string expectedFromCookie = "apples";
            var cl = ts.HttpClient;
            var url = String.Format("http://example.com/root/{0}", expectedFromUrl);
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Cookie", string.Format("noMatch={0}", expectedFromCookie));
            var resp = cl.SendAsync(request).Result;

            // Then: The route variable for the cookie is null
            Assert.IsTrue(resp.IsSuccessStatusCode);
            Assert.IsNull(_extractedCookie);   
        }

        private TestServer CreateTestServer()
        {
            TestFunc t1 = (env, fromUrl, fromCookie) => {
                _extractedUrl = fromUrl;
                _extractedCookie = fromCookie;
                var ctx = new OwinContext(env);
                return ctx.Response.WriteAsync("Some bananas");
            };
            var ts = Microsoft.Owin.Testing.TestServer.Create(app => {
                app.Use<RouteCookie>("fromCookie", "fromCookie", "outCookie");
                app.RouteGet(t1, "/root/<fromUrl>");
            });
            return ts;
        }
    }
}
        