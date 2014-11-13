namespace OwinUtils
{
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Owin;
    using Owin;
    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>; 

    /// <summary>
    /// Owin Middleware which converts from a value in the EnvDict to a signed session cookie
    /// and back again. Incorrectly signed cookies are ignored
    /// </summary>
    public class SessionCookie 
    {
        private static readonly char[] seperator = { ':' };
        private readonly string cookieName = "session";
        private byte[] passphrase;
        private string environmentKey;
        private AppFunc nextMiddleware;
        private Action<IOwinRequest, string> inboundFunc;
        private Func<IOwinResponse, string> outboundFunc;

        public SessionCookie(AppFunc next, string environmentKey, string passphrase)
        {
            this.nextMiddleware = next;
            this.passphrase = passphraseToBytes(passphrase);
            this.environmentKey = environmentKey;
            this.inboundFunc = this.injectSessionToRequest;
            this.outboundFunc = this.extractSessionFromResponse;
        }

        public SessionCookie(AppFunc next, string passphrase, Action<IOwinRequest, string> inbound, Func<IOwinResponse, string> outbound)
        {
            this.nextMiddleware = next;
            this.passphrase = passphraseToBytes(passphrase);
            this.inboundFunc = inbound;
            this.outboundFunc = outbound;
        }


        static byte[] passphraseToBytes(string passphrase)
        {
            return Encoding.UTF8.GetBytes(passphrase);
        }

        public Task Invoke(IDictionary<string, object> env)
        {
            return this.Invoke(new OwinContext(env));
        }

        void injectSessionToRequest(IOwinRequest request, string session)
        {
            request.Environment[this.environmentKey] = session;
        }

        string extractSessionFromResponse(IOwinResponse response)
        {
            if(response.Environment.ContainsKey(this.environmentKey) == false) {
                return null;
            }
            string outboundSession = response.Environment[this.environmentKey].ToString();
            return outboundSession;
        }
        
        public Task Invoke(IOwinContext context)
        {
            var request = context.Request;
            if (context.Request.Headers["cookie"] != null) {
                string beforeValue = request.Cookies[this.cookieName];
                if (beforeValue != null) {
                    string session = extract(beforeValue);
                    inboundFunc(request, session);
                }
            }
            context.Response.OnSendingHeaders(state => {
                var resp = (IOwinResponse)state;
                convertToCookie(resp);
            }, context.Response);
            return this.nextMiddleware.Invoke(context.Environment);
        }

        private static string signature(string value, byte[] passphrase)
        {
            Rfc2898DeriveBytes rfc = new Rfc2898DeriveBytes(value, passphrase);
            byte[] sha = rfc.GetBytes(8);
            string strSha = BitConverter.ToString(sha);
            return strSha;
           }

        public static string sign(string value, string passphrase)
        {
            return sign(value, passphraseToBytes(passphrase));
        }

        public static string sign(string value, byte[] passphrase)
        {
            string strSha = signature(value, passphrase);
            return strSha + seperator[0] + value;
        }

        public string extract(string signed)
        {
            string[] split = signed.Split(seperator, 2);
            if (split.Length != 2) {
                return null;
            }
            string strSha = split[0];
            string session = split[1];
            string chkSha = signature(session, this.passphrase);
            if(chkSha != strSha) {
                Console.WriteLine("Possible session tampering. Hashes dont match {0} and {1}", strSha, chkSha);
                return null;
            }
            return session;
        }

 

        private void convertToCookie(IOwinResponse response)
        {
            var outboundSession = outboundFunc(response);
            if(outboundSession != null) {
                // sign the cookie
                response.Cookies.Append(this.cookieName, sign(outboundSession, this.passphrase));
            }
        }
    }
}
