using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Testing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Owin;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;
using RouteDict = System.Collections.Generic.Dictionary<string, object>;
using EnvDict = System.Collections.Generic.IDictionary<string, object>;

namespace OwinUtils
{
    class RouteBodyTests
    {
        delegate Task BodyFunc(EnvDict env, JsonStream body);

        [Test]
        public void BasicTest()
        {

            // Given: An Route Function that takes a JsonStream and writes
            // it back to the client
            BodyFunc callee = (env, body) => {
                var ctx = new OwinContext(env);
                return ctx.Response.WriteAsync(body.ToString());
            };

            // Given: A pipeline that inserts a JsonStream into the route parameters
            // and calls the function above
            var app = TestServer.Create(builder =>
            {
                var key = "body";
                Func<Stream, object> converter = stream => new JsonStream(stream);
                builder.Use<RouteBody>("POST", converter, key);
                builder.RoutePost(callee, "/");
            });

            // When: we post a json payload to the route
            var bodyIn = "some text";
            var po = new JObject();
            po["v"] = bodyIn;
            var t1 = app.HttpClient.PostAsJsonAsync("http://xyz.com/", po);
            var response = t1.Result;

            // Then: The call succeeds and we get the same data back as we sent
            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.AreEqual(JsonConvert.SerializeObject(po), response.Content.ReadAsStringAsync().Result);
        }

        [TypeConverter(typeof(ConstructorTypeConverter<JsonStream, ConvertFromJson>))]
        class ConvertFromJson
        {
            public ConvertFromJson(JsonStream body)
            {
                this.s = body.ToString();
            }

            public string s
            { get; set; }
        }

        delegate Task JsonBodyFunc(EnvDict env, ConvertFromJson body);

        [Test]
        public void ConverterTest()
        {
            // Given: A route function that takes an argument that is
            // convertible from JsonStream (ConvertFromJson) and returns it
            // in the response body
            JsonBodyFunc callee = (env, body) =>
            {
                var ctx = new OwinContext(env);
                return ctx.Response.WriteAsync(body.s);
            };

            // Given: A pipeline that inserts a JsonStream into the route parameters
            // and calls the function above
            var app = TestServer.Create(builder =>
            {
                var key = "body";
                Func<Stream, object> converter = stream => new JsonStream(stream);
                builder.Use<RouteBody>("POST", converter, key);
                builder.RoutePost(callee, "/");
            });

            // When: we post a json payload to the route
            var bodyIn = "some text";
            var po = new JObject();
            po["v"] = bodyIn;
            var t1 = app.HttpClient.PostAsJsonAsync("http://xyz.com/", po);
            var response = t1.Result;

            // Then: The call succeeds and we get the same data back as we sent
            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.AreEqual(JsonConvert.SerializeObject(po), response.Content.ReadAsStringAsync().Result);
        }

        [Test]
        public void NoBodyForNonMatchingHttpMethod()
        {

            // Given: An Route Function that sends "banana" when the input body is null
            BodyFunc callee = (env, body) =>
            {
                var ctx = new OwinContext(env);
                var msg = body != null ? body.ToString() : "banana";
                return ctx.Response.WriteAsync(msg);
            };

            // Given: A pipeline that inserts a JsonStream into the route parameters
            // and calls the function above
            var app = TestServer.Create(builder =>
            {
                var key = "body";
                Func<Stream, object> converter = stream => new JsonStream(stream);
                builder.Use<RouteBody>("POST", converter, key);
                builder.RoutePost(callee, "/");
            });

            // When: we post a json payload to the route
            var bodyIn = "some text";
            var po = new JObject();
            po["v"] = bodyIn;
            var t1 = app.HttpClient.PostAsJsonAsync("http://xyz.com/", po);
            var response = t1.Result;

            // Then: The call succeeds and we get the same data back as we sent
            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.AreEqual(JsonConvert.SerializeObject(po), response.Content.ReadAsStringAsync().Result);
        }


    }
}
