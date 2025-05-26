using System;
using System.Security.Cryptography;
using System.Text;

namespace Codout.Framework.Common.Security;

public class Crypto
{
    [Obsolete("Obsolete")]
    public static string Md5Encrypt(string phrase)
    {
        var encoder = new UTF8Encoding();
        var md5Hasher = new MD5CryptoServiceProvider();
        var hashedDataBytes = md5Hasher.ComputeHash(encoder.GetBytes(phrase));
        return ByteArrayToString(hashedDataBytes);
    }

    [Obsolete("Obsolete")]
    public static string Sha1Encrypt(string phrase)
    {
        var encoder = new UTF8Encoding();
        var sha1Hasher = new SHA1CryptoServiceProvider();
        var hashedDataBytes = sha1Hasher.ComputeHash(encoder.GetBytes(phrase));
        return ByteArrayToString(hashedDataBytes);
    }

    [Obsolete("Obsolete")]
    public static string Sha256Encrypt(string phrase)
    {
        var encoder = new UTF8Encoding();
        var sha256Hasher = new SHA256Managed();
        var hashedDataBytes = sha256Hasher.ComputeHash(encoder.GetBytes(phrase));
        return ByteArrayToString(hashedDataBytes);
    }

    [Obsolete("Obsolete")]
    public static string Sha384Encrypt(string phrase)
    {
        var encoder = new UTF8Encoding();
        var sha384Hasher = new SHA384Managed();
        var hashedDataBytes = sha384Hasher.ComputeHash(encoder.GetBytes(phrase));
        return ByteArrayToString(hashedDataBytes);
    }

    [Obsolete("Obsolete")]
    public static string Sha512Encrypt(string phrase)
    {
        var encoder = new UTF8Encoding();
        var sha512Hasher = new SHA512Managed();
        var hashedDataBytes = sha512Hasher.ComputeHash(encoder.GetBytes(phrase));
        return ByteArrayToString(hashedDataBytes);
    }

    public static string ByteArrayToString(byte[] inputArray)
    {
        var output = new StringBuilder("");
        foreach (var t in inputArray)
            output.Append(t.ToString("X2"));
        return output.ToString();
    }
}