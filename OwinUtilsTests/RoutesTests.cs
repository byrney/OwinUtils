using ConsoleHost;

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
    public class RoutesTests : MiddlewareTestBase
    {

        public static Task SayHello(EnvDict env, EnvDict param)
        {
            var ctx = new OwinContext(env);
            return ctx.Response.WriteAsync("hello");
        }

        public static Task SayGoodbye(EnvDict env, EnvDict param)
        {
            var ctx = new OwinContext(env);
            return ctx.Response.WriteAsync("Goodbye");
        }
        public static Task SayBebe(EnvDict env, EnvDict param)
        {
            var ctx = new OwinContext(env);
            var v = param["bebe"] as string;
            return ctx.Response.WriteAsync(v);
        }

        [Test]
        public void blah()
        {
            var ts = TestServer.Create(app => {
                app.Route("/hola/<bebe>", SayBebe);
                app.Branch("/hello", b => b.Run(ctx => SayHello(ctx, null)));
                app.Route("/goodbye", SayGoodbye);
            });
            var cl = ts.HttpClient;
            var resp = cl.GetAsync("http://example.com/hola/boo").Result;
            Assert.IsTrue(resp.IsSuccessStatusCode);
            var content = resp.Content.ReadAsStringAsync().Result;
            Assert.AreEqual("boo", content);
        }

        [Test]
        public void blahblahblah()
        {
            var ts = TestServer.Create(app =>
            {
                app.Route("/hola/<bebe>", SayBebe);
                app.Branch("/hello", b => b.Run(ctx => SayHello(ctx, null)));
                app.Route("/goodbye", SayGoodbye);
            });
            var cl = ts.HttpClient;
            var resp = cl.GetAsync("http://example.com/hello/banana").Result;
            Assert.IsTrue(resp.IsSuccessStatusCode);
            var content = resp.Content.ReadAsStringAsync().Result;
            Assert.AreEqual("hello", content);
        }
    }
}
