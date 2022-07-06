using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Codout.Framework.Common.Security;

public class CryptoString
{
    private static byte[] _chave = { };
    private static readonly byte[] Iv = { 12, 33, 59, 85, 97, 101, 119, 122 };

    public static string Encrypt(string text, string key)
    {
        var des = new DESCryptoServiceProvider();
        var ms = new MemoryStream();

        var input = Encoding.UTF8.GetBytes(text);
        _chave = Encoding.UTF8.GetBytes(key[..8]);

        var cs = new CryptoStream(ms, des.CreateEncryptor(_chave, Iv), CryptoStreamMode.Write);
        cs.Write(input, 0, input.Length);
        cs.FlushFinalBlock();

        return Convert.ToBase64String(ms.ToArray());
    }

    public static string Decrypt(string text, string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentNullException("The argument \"key\" cannot be null");

        if (key.Length < 8)
            throw new ArgumentOutOfRangeException("The \"key\" parameter must have 8 characters");

        var des = new DESCryptoServiceProvider();
        var ms = new MemoryStream();

        var input = Convert.FromBase64String(text.Replace(" ", "+"));

        _chave = Encoding.UTF8.GetBytes(key[..8]);

        var cs = new CryptoStream(ms, des.CreateDecryptor(_chave, Iv), CryptoStreamMode.Write);
        cs.Write(input, 0, input.Length);
        cs.FlushFinalBlock();

        return Encoding.UTF8.GetString(ms.ToArray());
    }
}
