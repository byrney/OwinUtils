

namespace OwinUtils
{
    using System;
    using EnvDict = System.Collections.Generic.IDictionary<string, object>;

    public class ForwardedUrlBuilder : UrlBuilder
    {
        string host;
        string scheme;
        string scriptName;
        string basePath;
        string localRoot;

        public ForwardedUrlBuilder(EnvDict env)
        {
            var fdict = (EnvDict)env["forwarded"];
            this.host = (string)fdict["Host"];
            this.scheme = (string)fdict["Scheme"];
            this.scriptName = ((string)fdict["ScriptName"]).Trim('/');
            this.basePath = (string)env["owin.RequestPathBase"];
            this.localRoot = (string)fdict["LocalRoot"];
        }

        public string url(string relativePath)
        {
            var rp = relativePath;
            if(rp.StartsWith(this.localRoot)) {
                rp = rp.Remove(0, this.localRoot.Length).TrimStart('/');

            }
            var arr = new string[] { this.scriptName, rp };
            var path = String.Join("/", arr);
            return string.Format("{0}://{1}/{2}", this.scheme, this.host, this.scriptName, rp);

        }

        public string path(string relativePath)
        {
            throw new NotImplementedException();
        }

        public string root {
            get {
                throw new NotImplementedException();
            }
        }

    }
}

