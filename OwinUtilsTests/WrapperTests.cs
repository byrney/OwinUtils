
namespace OwinUtilsTests
{

    using NUnit.Framework;
    using System;
    using OwinUtils;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using EnvDict = System.Collections.Generic.IDictionary<string, object>;

	class Concatenate
	{
		public static bool StaticInvoke(EnvDict env, string name, string address)
		{
            env["result"] = name + address;
			Console.WriteLine("name = {0}, address = {1}", name, address);
            return true;
		}

		public Task InvokeMe(EnvDict env, string name, string address)
		{
            var t = new TaskCompletionSource<bool>();
            t.SetResult(StaticInvoke (env, name, address));
            return t.Task;
		}
        public Task InvokeMeInts(EnvDict env, int lhs, int rhs)
        {
            var t = new TaskCompletionSource<bool>();
            env["result"] = lhs + rhs;
            t.SetResult(true);
            return t.Task;
        }


	}




	[TestFixture ()]
	public class WrapperTests
	{
		string name;
		string address;
		string expected;
		Dictionary<string, object> dict;

		public Wrapper extract(Expression arg)
		{
			return new Wrapper (null);
		}

		public WrapperTests ()
		{
			this.name = "Robert";
			this.address = "22 Inglewood";
			this.expected = this.name + this.address;
			this.dict = new Dictionary<string, object> ();
			dict ["address"] = address;
			dict ["name"] = name;
            dict["lhs"] = "2";
            dict["rhs"] = "3";
		}

		[Test]
		public void TestClassInstance ()
		{
			var callee = new Concatenate ();
			var wrapper = new Wrapper (callee, "InvokeMe");
            EnvDict env = new Dictionary<string, object>();
            Task t = wrapper.InvokeRoute(env, this.dict);
            Assert.AreEqual(this.expected, (string)env["result"]);
		}

    
		delegate Task Del1(EnvDict env, string name, string address);
        delegate Task DelInt(EnvDict env, int lhs, int rhs);
        /*
		[Test]
		public void TestDelegate ()
		{
			Del1 f = (n, a) => { return n + a; };
			var wrapper = new Wrapper (f);
			string res  = wrapper.Invoke (dict);
			Assert.AreEqual(this.expected, res);
		}
*/

		[Test]
		public void TestDelegateToInstanceMethod ()
		{
			var c = new Concatenate ();
			Del1 f = c.InvokeMe;
            EnvDict env = new Dictionary<string, object>();
			var wrapper = new Wrapper (f);
            wrapper.InvokeRoute(env, this.dict).Wait();
            Assert.AreEqual(this.expected, (string)env["result"]);
		}

        [Test]
        public void TestTypeConversion ()
        {
            var c = new Concatenate ();
            DelInt f = c.InvokeMeInts;
            EnvDict env = new Dictionary<string, object>();
            var wrapper = new Wrapper (f);
            wrapper.InvokeRoute(env, this.dict).Wait();
            Assert.AreEqual(5, (int)env["result"]);
        }

	/*	
		delegate string ParamDel(params object[] p);

		[Test]
		public void TestStaticMethod ()
		{
			ParamDel q = Concatenate.StaticInvoke;
			var wrapper = new Wrapper (q);
			string res  = wrapper.Invoke (dict);
			Assert.AreEqual(this.expected ,res);

		}

		[Test]
		public void TestInstanceMethod ()
		{

			var callee = new Concatenate ();
			Func<string> x = callee.Invoke;
			var f = callee.Invoke;
		//	var wrapper = new Wrapper (callee.Invoke);
			string res  = wrapper.Invoke (dict);
			Assert.AreEqual(this.expected ,res);

		}
*/

	}
}

