    using System;
    using OwinUtils;
    using Owin;
    using Microsoft.Owin;
    using EnvDict = System.Collections.Generic.IDictionary<string, object>;
    using System.Threading.Tasks;
    
    namespace OwinUtils.Samples
    {
        public class RouteQuerySample
        {
            public delegate Task WriteQueryFunc(IOwinContext ctx, int queryParameter);
    
            // create instance of delegate with lambda
            static private WriteQueryFunc writeParam = (ctx, p) => ctx.Response.WriteAsync(string.Format("Query Param: {0}\n", p));
    
            public static void BuildSample(IAppBuilder app)
            {
                // extract queryParameter from URL.  Default to null  (zero for int)
                app.RouteQuery("queryParameter", null);
                // args of delegate will be matched and converted
                app.RouteGet(writeParam, "/", "");
            }
    
        }
    }
    
