namespace OwinSession.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Owin;
    using NUnit.Framework;
    using Moq;
    using OwinSession;
    using EnvDict = System.Collections.Generic.Dictionary<string, object>;
    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>; 

    public class SessionMiddlewareTests
    {
        private string envKey = "test.session";
        private string passphrase = "a passphrase......";

        [Test]
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

        [Test]
        public void CanInvoke()
        {
            string envKey = "test.session";
            string passphrase = "a passphrase......";
            var next = new AppFunc(e => DoNothing(e));
            var subject = new SessionMiddleware(next, envKey, passphrase);
            var dict = new EnvDict();
            var headers = new Dictionary<string, string[]>();
            dict["owin.RequestHeaders"] = headers;
            int beforeCount = dict.Count;
            subject.Invoke(dict);
            Assert.AreEqual(beforeCount, dict.Count);
        }

        public Task SetEnvKey(IDictionary<string, object> e)
        {
            e[this.envKey] = "a value";
            var tcs = new TaskCompletionSource<bool>();
            tcs.SetResult(true);
            return tcs.Task;
        }

        [Test]
        public void CookieGetsSet()
        {
            var next = new AppFunc(e => SetEnvKey(e));
            var subject = new SessionMiddleware(next, envKey, passphrase);
            var dict = new EnvDict();
            var headers = new Dictionary<string, string[]>();
            dict["owin.RequestHeaders"] = headers;
            dict["owin.ResponseHeaders"] = headers;
            var afterHeaders = dict["owin.ResponseHeaders"] as IDictionary<string, string[]> ;
            Assert.IsNotNull(afterHeaders);
            Assert.IsNotNull(afterHeaders["set-cookie"]);
        }

       

    }
}