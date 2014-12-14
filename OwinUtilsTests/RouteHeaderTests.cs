
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OwinUtilsTests
{

    using Microsoft.Owin;
    using Microsoft.Owin.Testing;
    using NUnit.Framework;
    using OwinUtils;
    using System.Net.Http;
    using AppFunc = Func<IDictionary<string, object>, Task>;
    using EnvDict = IDictionary<string, object>;
    using HeaderDict = IDictionary<string, string[]>;

    public class RouteHeaderTests
    {

        private delegate Task ReturnHeaderFunc(EnvDict env, string headerValue);

        [Test]
        public void CanExtractHeaderIntoRoute()
        {
            // Given: A server that returns the value of a header "header" in the response Body
            ReturnHeaderFunc f = (env, headerValue) => new OwinContext(env).Response.WriteAsync(headerValue);
            var app = TestServer.Create(builder => {
                builder.RouteHeader("header", "headerValue");
                builder.RouteGet(f, "/", "");
            });
       
            // when: a request with "header" header is sent
            var request = new HttpRequestMessage(HttpMethod.Get, "http://xyz.com/");
            const string expected = "headervalue";
            request.Headers.Add("header", expected);
            var response = app.HttpClient.SendAsync(request).Result;

            // then: the call succeeeds and the body returned matches the header sent in the request
            Assert.IsTrue(response.IsSuccessStatusCode);
            var respBody = response.Content.ReadAsStringAsync().Result;
            Assert.AreEqual(expected, respBody);
        }


        [Test]
        public void MissingHeadersDoNotOverwriteTheRouteParams()
        {
            // Given: A server that returns the value of a header1 or header2 "header" in the response Body
            ReturnHeaderFunc f = (env, headerValue) => new OwinContext(env).Response.WriteAsync(headerValue);
            var app = TestServer.Create(builder =>
            {
                builder.RouteHeader("header1", "headerValue");
                builder.RouteHeader("header2", "headerValue");
                builder.RouteGet(f, "/", "");
            });

            // when: a request with "header1" and NO header2 is sent
            var request = new HttpRequestMessage(HttpMethod.Get, "http://xyz.com/");
            const string expected = "headervalue";
            request.Headers.Add("header1", expected);
            var response = app.HttpClient.SendAsync(request).Result;

            // then: the call succeeeds and the body returned matches the header1 sent in the request
            Assert.IsTrue(response.IsSuccessStatusCode);
            var respBody = response.Content.ReadAsStringAsync().Result;
            Assert.AreEqual(expected, respBody);
        }

    }

 
}

