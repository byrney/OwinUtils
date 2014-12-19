using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OwinUtilsTests
{
    public class JsonStream
    {
        public JsonStream(Stream s)
        {
            this.Stream = s;
        }

        public Stream Stream { get; set; }

        public string ReadAsString()
        {
            using (var sr = new StreamReader(this.Stream, Encoding.UTF8))
            {
                return sr.ReadToEnd();
            }
        }
    }
}
