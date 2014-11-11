using System;
using OwinUtils;

namespace ConsoleHost
{
    public class HasDelegates
    {
        public HasDelegates()
        {
        }

        public delegate string DsFunc(string one, int two);

        public DsFunc DoSomething { get { return doSomething; }}

        public DsFunc DoAnother = (x,t) => "bye";

        public string doSomething(string one, int two)
        {
            Type t = typeof(string[]);
            return "hi";
        }

    }

    public class Builder
    {

        void Take(Delegate x)
        {
            var w = new Wrapper(x);
        }

        void foo() 
        {
            var c = new HasDelegates();
            Take(c.DoSomething);
        }
    }

}

