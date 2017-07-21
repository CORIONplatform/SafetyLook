using System;
using System.IO;
using System.Security.Cryptography;
using SjclHelpers.Codec;

namespace SjclHelpers.Mode
{
    internal static class Ccm
    {
        /// <summary>
        /// Creates an instance of CcmCipherText
        /// which is able to decrypt the specified cipher text.
        /// </summary>
        /// <param name="aesBlockCipher"></param>
        /// <param name="cipherText"></param>
        /// <param name="nonce"></param>
        /// <param name="messageAuthenticationCodeLength"></param>
        /// <returns></returns>
        internal static CcmCipherText Decrypt(
            Aes aesBlockCipher,
            byte[] cipherText,
            byte[] nonce,
            int messageAuthenticationCodeLength
        )
        {
            return new CcmCipherText(
                aesBlockCipher,
                cipherText,
                nonce,
                messageAuthenticationCodeLength
            );
        }
    }

}