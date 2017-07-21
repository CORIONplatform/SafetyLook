using System;
using System.Text;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;

namespace SjclHelpers
{
    public static class Encryption
    {
        /// <summary>
        /// Decrypts the cipher text.
        /// </summary>
        /// <param name="cipherText">The cipher text.</param>
        /// <param name="key">The encryption key.</param>
        /// <param name="initializationVector">The IV.</param>
        /// <returns>The decrypted text.</returns>
        public static string Decrypt(
            this byte[] cipherText,
            byte[] key,
            byte[] initializationVector
        )
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (initializationVector == null)
            {
                throw new ArgumentNullException("key");
            }
            var keyParameter = new KeyParameter(key);
            const int macSize = 64;
            var nonce = initializationVector;
            byte[] associatedText = null;
            var ccmParameters = new CcmParameters(
                keyParameter,
                macSize,
                nonce,
                associatedText
            ); 

            var ccmMode = new CcmBlockCipher(new AesFastEngine());
            var forEncryption = false;
            ccmMode.Init(forEncryption, ccmParameters);
            var plainBytes =
                new byte[ccmMode.GetOutputSize(cipherText.Length)];
            var res = ccmMode.ProcessBytes(
                cipherText, 0, cipherText.Length, plainBytes, 0);
            var numberOfBytes = ccmMode.DoFinal(plainBytes, res);
            return Encoding.UTF8.GetString(plainBytes);
        }
    }
}
