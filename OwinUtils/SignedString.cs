using System;
using System.Security.Cryptography;
using System.Text;

namespace OwinUtils
{
    /// <summary>
    /// Utility class for signing strings with a hash and then checking them 
    /// when extracting the original content
    /// </summary>
    public class SignedString
    {
        private static readonly char[] seperator = { ':' };

        public static string signature(string value, byte[] passphrase)
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

        public static string extract(string signedSession, byte[] passphrase)
        {
            string[] split = signedSession.Split(seperator, 2);
            if (split.Length != 2)
            {
                return null;
            }
            string strSha = split[0];
            string session = split[1];
            string chkSha = signature(session, passphrase);
            if (chkSha != strSha)
            {
                Console.WriteLine("Possible session tampering. Hashes dont match {0} and {1}", strSha, chkSha);
                return null;
            }
            return session;
        }

        public static byte[] passphraseToBytes(string passphrase)
        {
            return Encoding.UTF8.GetBytes(passphrase);
        }

    }
}