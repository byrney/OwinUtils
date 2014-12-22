using System;
using System.IO;
using System.Collections.Generic;

namespace OwinUtils
{
    /// <summary>
    /// Us this as the return value from a route
    /// that can resturn headers and body synchronously
    /// </summary>
    public class RouteReturn
    {
        public RouteReturn(string stringBody)
        {
            this.StringBody = stringBody;
        }
        public RouteReturn(Stream streamBody)
        {
            this.StreamBody = streamBody;
        }

        public int Status { get ; set;}
        public IDictionary<string, string[]> Headers { get; set; }
        public string StringBody { get; set; }
        public Stream StreamBody { get ; set; }
    }
}

    