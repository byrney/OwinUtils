

namespace OwinUtilsTests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Net.Http;
    using Microsoft.Owin;
    using Microsoft.Owin.Testing;
    using NUnit.Framework;
    using Owin;
    using OwinUtils;
    using EnvDict = System.Collections.Generic.IDictionary<string, object>;
    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;
    using System.IO;


    [TestFixture]
    public class EventSourceTests : MiddlewareTestBase
    {

        [Test]
        public void normalResponseBodyIsReturnedThroughMiddleware()
        {
            var app = new EventTestApp();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://xyz.com/");
            var response = testServerGet(app.noEvents, request);
            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.AreEqual(app.responseBody, response.Content.ReadAsStringAsync().Result);
        }
  
        [Test]
        public void eventSOurceIsInEnvironmentDict()
        {
            var app = new EventTestApp();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://xyz.com/");
            var response = testServerGet(app.noEvents, request);
            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.AreEqual(app.responseBody, response.Content.ReadAsStringAsync().Result);
        }
            

        [Test]
        public void writeAfterReturnWorks()
        {
            var app = new EventTestApp();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://xyz.com/");
            using (var server = TestServer.Create(app.openStreamReturnAndWrite)) {
                var client = server.HttpClient;
                var response = client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).Result;
                Assert.IsTrue(response.IsSuccessStatusCode);
                using (var events = response.Content.ReadAsStreamAsync().Result) {
                    using (var reader = new StreamReader(events)) {
                        var line = reader.ReadLine();
                        Assert.AreEqual(app.responseBody, line);
                    }
                }
            }
        }


        [Test]
        public void headersAreSetIfStreamIsOpened()
        {
            var app = new EventTestApp();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://xyz.com/");
            var response = testServerGet(app.openClose, request, true);
            Assert.IsTrue(response.IsSuccessStatusCode);
            var ct = response.Content.Headers.ContentType.ToString();
            Assert.AreEqual("text/eventstream", ct);
        }

        [Test]
        public void onCloseGetsCalledOnceWhenServerClosesImmediately()
        {
            var app = new EventTestApp();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://xyz.com/");
            var response = testServerGet(app.openClose, request, true);
            Assert.AreEqual(1, app.onCloseCalledCount);
        }

        [Test]
        public void onCloseGetsCalledOnceWhenServerClosesAfterReturn()
        {
            var app = new EventTestApp();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://xyz.com/");
            var response = testServerGet(app.openStreamAndWrite, request, true);
            app.stream.Close();
            Assert.AreEqual(1, app.onCloseCalledCount);
        }

        [Test]
        public void onCloseGetsCalledOnceClientDisconnects()
        {
            var app = new EventTestApp();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://xyz.com/");
            using (var server = TestServer.Create(app.openStream)) {
                var client = server.HttpClient;
                client.Timeout = new TimeSpan(0, 0, 0, 0, 100);
                var response = client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead)
                    .ContinueWith(_ => 
                        Assert.AreEqual(1, app.onCloseCalledCount)
                               );
            }
        }




    }
}