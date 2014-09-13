namespace OwinSession
{
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Owin;
    using Owin;
    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>; 

    public class SessionMiddleware
    {
        private readonly char[] seperator = { ':' };
        private readonly string cookieName = "session";
        private byte[] passphrase;
        private string environmentKey;
        private AppFunc nextMiddleware;

        public SessionMiddleware(AppFunc next, string environmentKey, string passphrase)
        {
            this.nextMiddleware = next;
            this.passphrase = Encoding.UTF8.GetBytes(passphrase);
            this.environmentKey = environmentKey;
        }

        public Task Invoke(IDictionary<string, object> env)
        {
            return this.Invoke(new OwinContext(env));
        }
        
        public Task Invoke(IOwinContext context)
        {
            string cookies = context.Request.Headers["cookie"];
            string beforeValue = context.Request.Cookies[this.cookieName];
            if (beforeValue != null) {
                string session = extract(beforeValue);
                context.Request.Environment[this.environmentKey] = session;
            }
            return this.nextMiddleware.Invoke(context.Environment).ContinueWith(_ => convertToCookie(context.Response));
        }

    	private string signature(string value)
    	{
		    Rfc2898DeriveBytes rfc = new Rfc2898DeriveBytes(value, this.passphrase);
            byte[] sha = rfc.GetBytes(8);
		    string strSha = BitConverter.ToString(sha);
				return strSha;
           }

		private string sign(string value)
		{
		    string strSha = this.signature(value);
		    return strSha + this.seperator[0] + value;
		}

		private string extract(string signed)
        {
		    string[] split = signed.Split(this.seperator, 1);
            string strSha = split[0];
			string session = split[1];
        	string chkSha = this.sign(session);
		    if(chkSha != strSha) {
		    	return null;
		    }
		    return session;
		}

		private void convertToCookie(IOwinResponse context)
		{
		        string value = context.Environment[this.environmentKey].ToString();
		   		if(value != null) {
				// sign the cookie
					context.Cookies.Append(this.cookieName, this.sign(value));
			    }
		}
    }
}
