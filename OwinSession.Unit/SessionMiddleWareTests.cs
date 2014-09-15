namespace OwinSession.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Net.Http;
    using Microsoft.Owin;
    using Microsoft.Owin.Testing;
    using NUnit.Framework;
    using Moq;
    using Owin;
    using OwinSession;
    using EnvDict = System.Collections.Generic.Dictionary<string, object>;
    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>; 


    public class Startup
    {
        public static string envKey = "test.session";
        public static string passPhrase = "a pass phrase that is long";
        public static string message = "goodbye";
        public static string sessionData = "some data that is in the session";
        public static Func<IOwinContext, Task> appFunc = context => {
            var env = context.Environment;
            object inboundSession = env.ContainsKey(envKey) ? env[envKey] : null;
            context.Environment[envKey] = sessionData;
            return context.Response.WriteAsync(string.Format("{0}\n",  inboundSession));
        };
        public void Configuration(IAppBuilder app)
        {
            app.Use<OwinSession.SessionMiddleware>(envKey, passPhrase);
            app.Run(appFunc);
        }
    }


    [TestFixture]
    public class SessionMiddlewareTests
    {
        private string envKey = "test.session";
        private string passphrase = "a passphrase......";

        private TestServer server;
        private HttpClient client;

        [TestFixtureSetUp]
        public void Setup()
        {
            this.server = TestServer.Create<Startup>();
            this.client = server.HttpClient;
        }

        [TestFixtureTearDown]
        public void Shutdown()
        {
            if(this.server != null) {
                this.server.Dispose();
            }
        }

        [Test]
        public void IgnoresUnsignedInputCookies()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "http://xyz.com/");
            request.Headers.Add("Cookie", "session=random_value");
            var response = client.SendAsync(request).Result;
            Assert.IsTrue(response.IsSuccessStatusCode);
            string body = response.Content.ReadAsStringAsync().Result;
            Assert.AreEqual("\n", body);
        }

        [Test]
        public void ReturnsASignedValue()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "http://xyz.com/");
            string inputValue = "some stuff";
            var signed = SessionMiddleware.sign(inputValue, Startup.passPhrase);
            var encoded = Uri.EscapeDataString(signed);
            request.Headers.Add("Cookie", string.Format("session={0}", encoded));
            var response = client.SendAsync(request).Result;
            Assert.IsTrue(response.IsSuccessStatusCode);
            string body = response.Content.ReadAsStringAsync().Result;
            Assert.AreEqual(string.Format("{0}\n", inputValue), body);
        }


        [Test]
        public void SetsReturnCookie()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "http://xyz.com/");
            var response = client.SendAsync(request).Result;
            Assert.IsTrue(response.Headers.Contains("set-cookie"));
            var cookieHeader = response.Headers.GetValues("set-cookie");
            foreach (string c in cookieHeader) {
                var decoded = Uri.UnescapeDataString(c);
                if (decoded.StartsWith("session")) {
                    var split = decoded.Split(new char[]{':', ';'});
                    Assert.AreEqual(Startup.sessionData, split[1]);
                    return;
                }
            }
            Assert.Fail("Should not get here if session cookie has been set");
        }

        public void CanConstruct()
        {
            string envKey = "test.session";
            string passphrase = "a passphrase......";
            var next = new AppFunc(e => new Task(null));
            var subject = new SessionMiddleware(next, envKey, passphrase);
            Assert.NotNull(subject);
        }

        public Task DoNothing(IDictionary<string, object> e)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            tcs.SetResult(true);
          //  e["test.session"] = "some session data";
            return tcs.Task;
        }

        public Task SetEnvKey(IDictionary<string, object> e)
        {
            e[this.envKey] = "a value";
            var tcs = new TaskCompletionSource<bool>();
            tcs.SetResult(true);
            return tcs.Task;
        }

    }
}