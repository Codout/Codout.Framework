using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Codout.Framework.NetStandard.Commom.Security
{
    public class CryptoString
    {
        private static byte[] _chave = { };
        private static readonly byte[] Iv = { 12, 33, 59, 85, 97, 101, 119, 122 };

        private const string ChaveCriptografia = "C308D044FF164E979396D45AC44189A8";

        public static string Encrypt(string phrase)
        {
            var des = new DESCryptoServiceProvider();
            var ms = new MemoryStream();

            var input = Encoding.UTF8.GetBytes(phrase);
            _chave = Encoding.UTF8.GetBytes(ChaveCriptografia.Substring(0, 8));

            var cs = new CryptoStream(ms, des.CreateEncryptor(_chave, Iv), CryptoStreamMode.Write);
            cs.Write(input, 0, input.Length);
            cs.FlushFinalBlock();

            return Convert.ToBase64String(ms.ToArray());
        }

        public static string Decrypt(string phrase)
        {
            var des = new DESCryptoServiceProvider();
            var ms = new MemoryStream();

            byte[] input = Convert.FromBase64String(phrase.Replace(" ", "+"));

            _chave = Encoding.UTF8.GetBytes(ChaveCriptografia.Substring(0, 8));

            var cs = new CryptoStream(ms, des.CreateDecryptor(_chave, Iv), CryptoStreamMode.Write);
            cs.Write(input, 0, input.Length);
            cs.FlushFinalBlock();

            return Encoding.UTF8.GetString(ms.ToArray());
        }
    }
}
