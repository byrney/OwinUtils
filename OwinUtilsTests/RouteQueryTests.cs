
namespace OwinUtilsTests
{

    using Microsoft.Owin;
    using Microsoft.Owin.Testing;
    using NUnit.Framework;
    using Owin;
    using OwinUtils;
    using System.Net.Http;
    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;
    using RouteDict = System.Collections.Generic.Dictionary<string, object>;


    public class RouteQueryTests
    {

        [Test]
        public void CanExtractMidleware()
        {
            var app = TestServer.Create(builder => {
                var key = "pq";
                builder.Use<RouteQuery>(key, null);
                AppFunc handler = env =>
                {
                    var ctx = new OwinContext(env);
                    var v = RouteParams.GetParam<string>(env, key);
                    return ctx.Response.WriteAsync(v);
                };
                builder.Run(handler);
            });
            var value = "valueOfQueryParameter";
            var request = new HttpRequestMessage(HttpMethod.Get, "http://xyz.com/blah?pq=" + value);
            var response = app.HttpClient.SendAsync(request).Result;
            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.AreEqual(value, response.Content.ReadAsStringAsync().Result);
        }

        [Test]
        public void UsesDefaultIfNoQuery()
        {
            var defaultValue = "a default";
            var app = TestServer.Create(builder =>
            {
                var key = "pq";
                builder.Use<RouteQuery>(key, defaultValue);
                AppFunc handler = env =>
                {
                    var ctx = new OwinContext(env);
                    var v = RouteParams.GetParam<string>(env, key);
                    return ctx.Response.WriteAsync(v);
                };
                builder.Run(handler);
            });
            var value = "valueOfQueryParameter";
            var request = new HttpRequestMessage(HttpMethod.Get, "http://xyz.com/blah?qr=" + value);
            var response = app.HttpClient.SendAsync(request).Result;
            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.AreEqual(defaultValue, response.Content.ReadAsStringAsync().Result);
        }

    }


}

