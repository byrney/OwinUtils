namespace OwinUtils
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Owin;
    using Owin;
    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

    /// <summary>
    /// Extends IAppBuilder with signed session cookie Middleware
    /// </summary>
    public static class SessionCookieBuilder
    {
        /// <summary>Treat part of the Owin Environment as a tamper proof session cookie</summary>
        /// <remarks>
        /// Inbound: Extracts a string from a cookie called "session", checks that it has been signed using passphrase
        /// and adds it to the owin Environment under "environmentKey". If the cookie has not been signed
        /// using passphrase it will be rejected  (ignored) an the request will continue
        /// 
        /// Outbound: Gets a session string from the owin environment under "environmentKey" signs it using 
        /// passphrase and returns to the client in a cookie called "session"
        /// </remarks>
        /// <param name="iab">The this being extended</param>
        /// <param name="environmentKey">Where the read/write the cookie in the Owin Environment</param>
        /// <param name="passphrase">Passphrase to use when signing the cookie</param>
        public static IAppBuilder SessionCookie(this IAppBuilder iab, string environmentKey, string passphrase)
        {
            return iab.Use<SessionCookie>(environmentKey, passphrase);
        }
    }

 
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

}
