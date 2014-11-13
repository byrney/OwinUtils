
using System.Threading.Tasks;

namespace OwinUtilsTests
{
    using Microsoft.Owin;
    using Microsoft.Owin.Testing;
    using NUnit.Framework;
    using OwinUtils;
    using System;
    using System.Net;
    using Task = System.Threading.Tasks.Task;
    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, Task>;
    using EnvDict = System.Collections.Generic.IDictionary<string, object>;
    using RouteDict = System.Collections.Generic.Dictionary<string, object>;

    [TestFixture]
    public class RoutesTests : MiddlewareTestBase
    {

        public static Task SayHello(EnvDict env)
        {
            var ctx = new OwinContext(env);
            return ctx.Response.WriteAsync("hello");
        }

        public static Task TakesAContext(IOwinContext ctx)
        {
            return ctx.Response.WriteAsync("hello");
        }


        public static Task SayGoodbye(EnvDict env)
        {
            var ctx = new OwinContext(env);
            return ctx.Response.WriteAsync("Goodbye");
        }
        public static Task UseRouteDict(EnvDict env)
        {
            var ctx = new OwinContext(env);
            var v = RouteParams.GetParam<string>(env, "bebe");
            return ctx.Response.WriteAsync(v);
        }

        public static Task UseParameters(EnvDict env, string bebe)
        {
            var ctx = new OwinContext(env);
            return ctx.Response.WriteAsync(bebe);
        }

        public static Task SubIntegers(EnvDict env, int lhs, int rhs)
        {
            var ctx = new OwinContext(env);
            var res = lhs - rhs;
            return ctx.Response.WriteAsync(res.ToString());
        }

        public static Task AddIntegers(EnvDict env, int lhs, int rhs)
        {
            var ctx = new OwinContext(env);
            var res = lhs + rhs;
            return ctx.Response.WriteAsync(res.ToString());
        }

        public static Task Square(EnvDict env, int value)
        {
            var ctx = new OwinContext(env);
            var res = value * value;
            return ctx.Response.WriteAsync(res.ToString());
        }

        [Test]
        public void RouteDictIsPassedCorrectly()
        {
            var ts = TestServer.Create(app => {
                app.Branch("/hello", b => b.Run(ctx => SayHello(ctx)));
                app.Route("/hola/<bebe>", UseRouteDict);
                app.Route("/goodbye", SayGoodbye);
            });
            var cl = ts.HttpClient;
            var resp = cl.GetAsync("http://example.com/hola/boo").Result;
            Assert.IsTrue(resp.IsSuccessStatusCode);
            var content = resp.Content.ReadAsStringAsync().Result;
            Assert.AreEqual("boo", content);
        }

        delegate Task ConcatFunc(EnvDict env, string name);
        [Test]
        public void UseWrapper()
        {
            ConcatFunc del = UseParameters;
            var ts = TestServer.Create(app =>
            {
                
                app.Route("/hola/<name>", del, "GET");
                app.Branch("/hello", b => b.Run(SayHello));
                app.Route("/goodbye", SayGoodbye);
            });
            var cl = ts.HttpClient;
            var resp = cl.GetAsync("http://example.com/hola/boo").Result;
            Assert.IsTrue(resp.IsSuccessStatusCode);
            var content = resp.Content.ReadAsStringAsync().Result;
            Assert.AreEqual("boo", content);
        }

        [Test]
        public void PassContext()
        {
            var ts = TestServer.Create(app => {
                var t = new RoutesTests();
                app.Route("/hola/boo", t, "TakesAContext");
            });
            var cl = ts.HttpClient;
            var resp = cl.GetAsync("http://example.com/hola/boo").Result;
            Assert.IsTrue(resp.IsSuccessStatusCode);
        }


        delegate Task AddFunc(EnvDict env, int lhs, int rhs);

        [Test]
        public void UseAddViaDelegate()
        {
            AddFunc add = AddIntegers;
            var ts = TestServer.Create(app =>
            {
                app.Route("/add/[lhs]/[rhs]", add, "Invoke", "GET");
                
            });
            var cl = ts.HttpClient;
            var resp = cl.GetAsync("http://example.com/add/5/6").Result;
            Assert.IsTrue(resp.IsSuccessStatusCode);
            var content = resp.Content.ReadAsStringAsync().Result;
            Assert.AreEqual("11", content);
        }

        [Test]
        public void UseAddViaFunc()
        {
            var ts = TestServer.Create(app =>
            {
                app.Route("/add/[arg2]/[arg3]", new Func<EnvDict, int, int, Task>(AddIntegers), "GET");

            });
            var cl = ts.HttpClient;
            var resp = cl.GetAsync("http://example.com/add/5/6").Result;
            Assert.IsTrue(resp.IsSuccessStatusCode);
            var content = resp.Content.ReadAsStringAsync().Result;
            Assert.AreEqual("11", content);
        }

        [Test]
        public void HitsCorrectRoute()
        {
            var ts = TestServer.Create(app =>
            {
                app.Route("/hola/<bebe>", UseRouteDict);
                app.Branch("/hello", b => b.Run(ctx => SayHello(ctx)));
            });
            var cl = ts.HttpClient;
            var resp = cl.GetAsync("http://example.com/hello/banana").Result;
            Assert.IsTrue(resp.IsSuccessStatusCode);
            var content = resp.Content.ReadAsStringAsync().Result;
            Assert.AreEqual("hello", content);
        }


        delegate Task SqFunc(EnvDict env, int value);

        [Test]
        public void BranchesAdjustBasePath()
        {
            AddFunc add = AddIntegers;
            var ts = TestServer.Create(app => {
                app.Branch("/hello", b => {
                    b.Route("/<lhs>/<rhs>", add, "GET");
                });
            });
            var cl = ts.HttpClient;
            var resp = cl.GetAsync("http://example.com/hello/9/10").Result;
            Assert.IsTrue(resp.IsSuccessStatusCode);
            var content = resp.Content.ReadAsStringAsync().Result;
            Assert.AreEqual("19", content);
        }


        [Test]
        [TestCase("http://example.com/branch/route")]
        [TestCase("http://example.com/branch")]
        [TestCase("http://example.com/branch/")]
        public void AbsoluteUrlsReconstructCorrectlyUnderRoutes(string requestedUrl)
        {
            AppFunc printAbsUrl = env => {
                var c = new OwinContext(env);
                return c.Response.WriteAsync(c.Request.Uri.AbsoluteUri);
            };
            var ts = TestServer.Create(app => 
                app.Branch("/branch", b =>
                    b.Route("/"     , printAbsUrl)
                     .Route("/route", printAbsUrl)
                     .Route(""      , printAbsUrl))
            );
            var cl = ts.HttpClient;
            var resp = cl.GetAsync(requestedUrl).Result;
            Assert.IsTrue(resp.IsSuccessStatusCode);
            var content = resp.Content.ReadAsStringAsync().Result;
            Assert.AreEqual(requestedUrl, content);
        }

        [Test]
        [TestCase("http://example.com/branch/")]
        public void RouteMatchesSingleSlashs(string requestedUrl)
        {
            AppFunc printOneFunc = env =>
            {
                var c = new OwinContext(env);
                return c.Response.WriteAsync("ONE");
            };

            AppFunc printTwoFunc = env =>
            {
                var c = new OwinContext(env);
                return c.Response.WriteAsync("TWO");
            };

            var ts = TestServer.Create(app =>
                app.Branch("/branch", b =>
                    b.Route("/", printOneFunc)
                     .Route("/[var]", printTwoFunc))
                );
            var cl = ts.HttpClient;
            var resp = cl.GetAsync(requestedUrl).Result;
            Assert.IsTrue(resp.IsSuccessStatusCode);
            var content = resp.Content.ReadAsStringAsync().Result;
            Assert.AreEqual("ONE", content);
        }

        [Test]
        [TestCase("http://example.com/branch/")]
        [TestCase("http://example.com/branch")]
        public void RouteMatchesOptVar(string requestedUrl)
        {
            Func<EnvDict, string, Task> printFunc = (env, value) => {
                var c = new OwinContext(env);
                return c.Response.WriteAsync(value);
            };
   
            var ts = TestServer.Create(app => {
                    app.Branch("/branch", b => {
                        b.Route("/", (env) => printFunc(env, "ONE"));
                        b.Route("", (env) => printFunc(env, "ONE"));
                        b.Route("/[var]", (env) => printFunc(env, "TWO"));
                    });

            });
            var cl = ts.HttpClient;
            var resp = cl.GetAsync(requestedUrl).Result;
            Assert.IsTrue(resp.IsSuccessStatusCode);
            var content = resp.Content.ReadAsStringAsync().Result;
            Assert.AreEqual("ONE", content);
        }

        [Test]
        [TestCase("http://example.com/branch/")]
        public void RouteMatchesOptVar2(string requestedUrl)
        {
            Func<EnvDict, string, Task> printFunc = (env, value) =>
            {
                var c = new OwinContext(env);
                return c.Response.WriteAsync(value);
            };

            var ts = TestServer.Create(app =>
            {
                app.Branch("/branch/", b =>
                {
                    b.Route("/", (env) => printFunc(env, "TWO"));
                    b.Route("", (env) => printFunc(env, "ONE"));
                    b.Route("/[var]", (env) => printFunc(env, "TWO"));
                });

            });
            var cl = ts.HttpClient;
            var resp = cl.GetAsync(requestedUrl).Result;
            Assert.IsTrue(resp.IsSuccessStatusCode);
            var content = resp.Content.ReadAsStringAsync().Result;
            Assert.AreEqual("ONE", content);
        }



        [Test]
        public void SkipsUnmatchedRoute()
        {
            AddFunc add = AddIntegers;
            SqFunc square = Square;
            var ts = TestServer.Create(app =>
            {
                app.Branch("/hello", b => {
                    b.Route("/<lhs>/<rhs>", add, "GET");
                    b.Route("/<value>", square, "GET");
                });
                app.Branch("goodbye", b => {
                    b.Route("/<value>", square, "GET");
                });
                    
            });
            var cl = ts.HttpClient;
            var resp = cl.GetAsync("http://example.com/hello/3").Result;
            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
            var content = resp.Content.ReadAsStringAsync().Result;
            Assert.AreEqual("9", content);
        }

        [Test]
        public void HttpMethodsAreMatched()
        {
            AddFunc add = AddIntegers;
            SqFunc square = Square;
           
            var ts = TestServer.Create(app =>
            {
                    app.Route("/hello", SayGoodbye, "POST");
                    app.Route("/hello", SayHello, "GET");
             
            });
            var cl = ts.HttpClient;
            var resp = cl.GetAsync("http://example.com/hello").Result;
            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
            var content = resp.Content.ReadAsStringAsync().Result;
            Assert.AreEqual("hello", content);
        }

    }
}


