namespace OwinUtils
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Owin;
    using Owin;
    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

    /// <summary>
    /// Owin Middleware which converts from a value in the EnvDict to a signed session cookie
    /// and back again. Incorrectly signed cookies are ignored
    /// </summary>
    class SessionCookie : SignedString
    {
       
        private byte[] passphrase;
        private string environmentKey;
        private const string cookieName = "session";
        private readonly CookieConverter converter;
        public SessionCookie(AppFunc next, string environmentKey, string passphrase)
        {
            this.converter = new CookieConverter(next, cookieName, this.injectSessionToRequest, this.extractSessionFromResponse);
            this.passphrase = passphraseToBytes(passphrase);
            this.environmentKey = environmentKey;
        }

        private SessionCookie(AppFunc next, string passphrase, Action<IOwinRequest, string> inbound, Func<IOwinResponse, string> outbound)
        {
            this.converter = new CookieConverter(next, cookieName, inbound, outbound);
            this.passphrase = passphraseToBytes(passphrase);
        }

        public Task Invoke(IDictionary<string, object> env)
        {
            return converter.Invoke(env);
        }

        void injectSessionToRequest(IOwinRequest request, string beforeValue)
        {
            string session = extract(beforeValue, this.passphrase);
            request.Environment[this.environmentKey] = session;
        }

        string extractSessionFromResponse(IOwinResponse response)
        {
            if(response.Environment.ContainsKey(this.environmentKey) == false) {
                return null;
            }
            string outboundSession = response.Environment[this.environmentKey].ToString();
            string signedSession = sign(outboundSession, this.passphrase);
            return signedSession;
        }
        


 
    }

    public static class AppBuilderSessionCookieExtensions
    {
        public static IAppBuilder SessionCookie(this IAppBuilder iab, string environmentKey, string passphrase)
        {
            return iab.Use<SessionCookie>(environmentKey, passphrase);
        }
    }

}
