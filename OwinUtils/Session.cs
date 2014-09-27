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

    public class Session 
    {
        private static readonly char[] seperator = { ':' };
        private readonly string cookieName = "session";
        private byte[] passphrase;
        private string environmentKey;
        private AppFunc nextMiddleware;

        public Session(AppFunc next, string environmentKey, string passphrase)
        {
            this.nextMiddleware = next;
            this.passphrase = passphraseToBytes(passphrase);
            this.environmentKey = environmentKey;
        }

        static byte[] passphraseToBytes(string passphrase)
        {
            return Encoding.UTF8.GetBytes(passphrase);
        }

        public Task Invoke(IDictionary<string, object> env)
        {
            return this.Invoke(new OwinContext(env));
        }
        
        public Task Invoke(IOwinContext context)
        {
            if (context.Request.Headers["cookie"] != null) {
                string beforeValue = context.Request.Cookies[this.cookieName];
                if (beforeValue != null) {
                    string session = extract(beforeValue);
                    context.Request.Environment[this.environmentKey] = session;
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

        private void convertToCookie(IOwinResponse context)
        {
            if (context.Environment.ContainsKey(this.environmentKey) == false) {
                return;
            }
            string value = context.Environment[this.environmentKey].ToString();
            if(value != null) {
                // sign the cookie
                context.Cookies.Append(this.cookieName, sign(value, this.passphrase));
            }
        }
    }
}
