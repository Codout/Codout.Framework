using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace Codout.Framework.Common.Helpers
{
    /// <summary>
    /// Classe responsável pela criptografia de arquivos.
    /// </summary>
    public class CryptoFile
    {
        #region ZeroMemory
        /// <summary>
        /// Call this function to remove the key from memory after use for security.
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        [DllImport("KERNEL32.DLL", EntryPoint = "RtlZeroMemory")]
        public static extern bool ZeroMemory(IntPtr destination, int length);
        #endregion

        #region GenerateKey
        /// <summary>
        /// Function to Generate a 64 bits Key.
        /// </summary>
        /// <returns></returns>
        public static string GenerateKey()
        {
            // Create an instance of Symetric Algorithm. Key and IV is generated automatically.
            var desCrypto = (DESCryptoServiceProvider)DES.Create();

            // Use the Automatically generated key for Encryption. 
            return Encoding.ASCII.GetString(desCrypto.Key);
        }
        #endregion

        #region EncryptFile
        /// <summary>
        /// Encripta um arquivo.
        /// </summary>
        /// <param name="sInputFilename"></param>
        /// <param name="sOutputFilename"></param>
        /// <param name="sKey"></param>
        public static void EncryptFile(string sInputFilename,
            string sOutputFilename,
            string sKey)
        {
            var fsInput = new FileStream(sInputFilename,
                FileMode.Open,
                FileAccess.Read);

            var fsEncrypted = new FileStream(sOutputFilename,
                FileMode.Create,
                FileAccess.Write);
            var des = new DESCryptoServiceProvider { Key = Encoding.ASCII.GetBytes(sKey), IV = Encoding.ASCII.GetBytes(sKey) };
            var desencrypt = des.CreateEncryptor();
            var cryptostream = new CryptoStream(fsEncrypted,
                desencrypt,
                CryptoStreamMode.Write);

            var bytearrayinput = new byte[fsInput.Length];
            fsInput.Read(bytearrayinput, 0, bytearrayinput.Length);
            cryptostream.Write(bytearrayinput, 0, bytearrayinput.Length);
            cryptostream.Close();
            fsInput.Close();
            fsEncrypted.Close();
        }
        #endregion

        #region DecryptFile
        /// <summary>
        /// Decripta um arquivo.
        /// </summary>
        /// <param name="sInputFilename"></param>
        /// <param name="sOutputFilename"></param>
        /// <param name="sKey"></param>
        public static void DecryptFile(string sInputFilename,
            string sOutputFilename,
            string sKey)
        {
            var des = new DESCryptoServiceProvider { Key = Encoding.ASCII.GetBytes(sKey), IV = Encoding.ASCII.GetBytes(sKey) };
            //A 64 bit key and IV is required for this provider.
            //Set secret key For DES algorithm.
            //Set initialization vector.

            //Create a file stream to read the encrypted file back.
            var fsread = new FileStream(sInputFilename,
                FileMode.Open,
                FileAccess.Read);
            //Create a DES decryptor from the DES instance.
            var desdecrypt = des.CreateDecryptor();
            //Create crypto stream set to read and do a 
            //DES decryption transform on incoming bytes.
            var cryptostreamDecr = new CryptoStream(fsread,
                desdecrypt,
                CryptoStreamMode.Read);
            //Print the contents of the decrypted file.
            var fsDecrypted = new StreamWriter(sOutputFilename);
            fsDecrypted.Write(new StreamReader(cryptostreamDecr).ReadToEnd());
            fsDecrypted.Flush();
            fsDecrypted.Close();
        }
        #endregion
    }
}
