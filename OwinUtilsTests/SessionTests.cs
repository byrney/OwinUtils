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


    [TestFixture]
    public class SessionTests : MiddlewareTestBase
    {
        private string envKey = "test.session";
        private string passphrase = "a passphrase......";



        [Test]
        public void responseBodyIsReturnedThroughMiddleware()
        {
            var app = new SessionTestApp();
            app.inboundSession = "before";
            var request = new HttpRequestMessage(HttpMethod.Get, "http://xyz.com/");
            request.Headers.Add("Cookie", "session=random_value");
            var response = testServerGet(app.CaptureInbound, request);
            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.AreEqual(app.responseBody, response.Content.ReadAsStringAsync().Result);
        }

        [Test]
        public void unsignedInputCookiesAreIgnored()
        {
            var app = new SessionTestApp();
            app.inboundSession = "before";
            var request = new HttpRequestMessage(HttpMethod.Get, "http://xyz.com/");
            request.Headers.Add("Cookie", "session=random_value");
            var response = testServerGet(app.CaptureInbound, request);
            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.IsNull(app.inboundSession);
        }

        [Test]
        public void signedInputCookiesAreAvailableDownstream()
        {
            var startApp = new SessionTestApp();
            string inputValue = "some stuff";
            var signed = Session.sign(inputValue, startApp.passPhrase);
            var encoded = Uri.EscapeDataString(signed);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://xyz.com/");
            request.Headers.Add("Cookie", string.Format("session={0}", encoded));
            var response = testServerGet(startApp.CaptureInbound, request);
            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.AreEqual(inputValue, startApp.inboundSession);
        }

        [Test]
        public void downstreamEnvBecomesOutputCookie()
        {
            var startApp = new SessionTestApp();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://xyz.com/");
            var response = testServerGet(startApp.SetOutbound, request);
            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.IsTrue(response.Headers.Contains("set-cookie"));
            var cookieName = "session";
            string cookieValue = extractCookie(response, cookieName);
            Assert.IsNotNull(cookieValue, "A cookie was expected but not found");
            Assert.AreEqual(startApp.sessionData, cookieValue);
        }

        public void CanConstruct()
        {
            string envKey = "test.session";
            string passphrase = "a passphrase......";
            var next = new AppFunc(e => new Task(null));
            var subject = new Session(next, envKey, passphrase);
            Assert.NotNull(subject);
        }
    }
}