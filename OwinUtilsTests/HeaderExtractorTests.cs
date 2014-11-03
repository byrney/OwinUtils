using System;

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
    public class HeaderExtractorTests 
    {

        [Test]
        public void CanUseMidleware()
        {
            bool called = false;
            var app = TestServer.Create(builder => {
                builder.Use<HeaderExtractor>("header");
                builder.Run(env => {
                    called = true;
                    return new OwinContext(env).Response.WriteAsync("sometext");
                });
            });
       
            var request = new HttpRequestMessage(HttpMethod.Get, "http://xyz.com/");
            var response = app.HttpClient.SendAsync(request).Result;
            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.AreEqual(true, called);
        }
    }

 
}

