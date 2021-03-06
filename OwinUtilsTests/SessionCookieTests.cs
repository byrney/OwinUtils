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
    public class SessionCookieTests : MiddlewareTestBase
    {
        private string envKey = "test.session";
        private string passphrase = "a passphrase......";



        [Test]
        public void responseBodyIsReturnedThroughMiddleware()
        {
            var app = new SessionCookieTestApp();
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
            var app = new SessionCookieTestApp();
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
            var startApp = new SessionCookieTestApp();
            string inputValue = "some stuff";
            var signed = SignedString.sign(inputValue, startApp.passPhrase);
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
            var startApp = new SessionCookieTestApp();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://xyz.com/");
            var response = testServerGet(startApp.SetOutbound, request);
            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.IsTrue(response.Headers.Contains("set-cookie"));
            var cookieName = "session";
            string cookieValue = extractCookie(response, cookieName);
            Assert.IsNotNull(cookieValue, "A cookie was expected but not found");
            Assert.AreEqual(startApp.sessionData, cookieValue);
        }


    }
}
