using System;
using System.IO;
using System.Collections.Generic;

namespace OwinUtils
{
    public class RouteReturn
    {
        public int Status { get ; set;}
        public IDictionary<string, string[]> Headers { get; set; }
        public string StringBody { get; set; }
        public Stream StreamBody { get ; set; }
    }
}

    