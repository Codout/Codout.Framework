using System.Security.Cryptography;
using System.Text;

namespace Codout.Framework.Common.Security
{
    public class Crypto
    {
        public static string Md5Encrypt(string phrase)
        {
            var encoder = new UTF8Encoding();
            var md5Hasher = new MD5CryptoServiceProvider();
            byte[] hashedDataBytes = md5Hasher.ComputeHash(encoder.GetBytes(phrase));
            return ByteArrayToString(hashedDataBytes);
        }

        public static string Sha1Encrypt(string phrase)
        {
            var encoder = new UTF8Encoding();
            var sha1Hasher = new SHA1CryptoServiceProvider();
            byte[] hashedDataBytes = sha1Hasher.ComputeHash(encoder.GetBytes(phrase));
            return ByteArrayToString(hashedDataBytes);
        }

        public static string Sha256Encrypt(string phrase)
        {
            var encoder = new UTF8Encoding();
            var sha256Hasher = new SHA256Managed();
            byte[] hashedDataBytes = sha256Hasher.ComputeHash(encoder.GetBytes(phrase));
            return ByteArrayToString(hashedDataBytes);
        }

        public static string Sha384Encrypt(string phrase)
        {
            var encoder = new UTF8Encoding();
            var sha384Hasher = new SHA384Managed();
            byte[] hashedDataBytes = sha384Hasher.ComputeHash(encoder.GetBytes(phrase));
            return ByteArrayToString(hashedDataBytes);
        }

        public static string Sha512Encrypt(string phrase)
        {
            var encoder = new UTF8Encoding();
            var sha512Hasher = new SHA512Managed();
            byte[] hashedDataBytes = sha512Hasher.ComputeHash(encoder.GetBytes(phrase));
            return ByteArrayToString(hashedDataBytes);
        }

        public static string ByteArrayToString(byte[] inputArray)
        {
            var output = new StringBuilder("");
            for (int i = 0; i < inputArray.Length; i++)
            {
                output.Append(inputArray[i].ToString("X2"));
            }
            return output.ToString();
        }
    }
}
