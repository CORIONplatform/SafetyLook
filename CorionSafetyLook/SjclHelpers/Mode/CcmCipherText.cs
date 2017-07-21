using System;
using System.Linq;
using System.Security.Cryptography;
using SjclHelpers.Codec;

namespace SjclHelpers.Mode
{
    internal class CcmCipherText
    {
        /// <summary>
        /// Get the minimum message authentication code (MAC) length.
        /// </summary>
        public const int MinimumMessageAuthenticationCodeLength = 4;

        /// <summary>
        /// Get the maximum message authentication code (MAC) length.
        /// </summary>
        public const int MaximumMessageAuthenticationCodeLength = 16;

        private readonly Aes _aesBlockCipher;
        private readonly byte[] _cipherText;
        private readonly int _messageAuthenticationCodeLength;
        private readonly byte[] _nonce;
        private readonly byte[] _messageAuthenticationCode;

        /// <summary>
        /// Get the AES bock cipher.
        /// </summary>
        internal virtual Aes AesBlockCipher
        {
            get { return _aesBlockCipher; }
        }

        /// <summary>
        /// Get the cipher text.
        /// </summary>
        internal virtual byte[] CipherText
        {
            get { return _cipherText; }
        }

        /// <summary>
        /// Get the message authenctication code length in number of bytes.
        /// </summary>
        internal virtual int MessageAuthenticationCodeLength
        {
            get { return _messageAuthenticationCodeLength; }
        }

        /// <summary>
        /// Get the nonce.
        /// </summary>
        internal virtual byte[] Nonce
        {
            get { return _nonce; }
        }

        /// <summary>
        /// Get the message length in number of bytes.
        /// </summary>
        internal virtual int MessageLength
        {
            get { return CipherText.Length - MessageAuthenticationCodeLength; }
        }

        /// <summary>
        /// Get the authentication field.
        /// </summary>
        internal virtual string AuthenticationField
        {
            get
            {
                var invalidMacLength =
                    (
                        MessageAuthenticationCodeLength <
                        MinimumMessageAuthenticationCodeLength
                    ) ||
                    (
                        MessageAuthenticationCodeLength >
                        MaximumMessageAuthenticationCodeLength
                    ) ||
                    MessageAuthenticationCodeLength % 2 == 1;
                if (invalidMacLength)
                {
                    throw new InvalidOperationException(
                        "Invalid message authentication code (MAC) length."
                    );
                }
                var fieldValue = (MessageAuthenticationCodeLength - 2) / 2;
                if (fieldValue < 1 || fieldValue > 7)
                {
                    throw new Exception("Implementation error.");
                }
                return Convert.ToString(fieldValue, 2).PadLeft(3, '0');
            }
        }

        /// <summary>
        /// Get the size of the length field in number of bytes.
        /// </summary>
        internal virtual int LengthFieldSize
        {
            get
            {
                var size = 15 - Nonce.Length;
                if (size < 2 || size > 8)
                {
                    throw new InvalidOperationException(
                        "Invalid nonce length."
                    );
                }
                return size;
            }
        }

        /// <summary>
        /// Get the first block.
        /// </summary>
        internal byte[] InitializationVector
        {
            get
            {
                string flagBits =
                    "00" +
                    AuthenticationField +
                    Convert
                        .ToString(LengthFieldSize - 1, 2)
                        .PadLeft(3, '0');
                var tmp = new byte[16];
                tmp[0] = Convert.ToByte(flagBits, 2);
                Array.Copy(Nonce, 0, tmp, 1, Nonce.Length);
                var messageLengthBytes = Convert
                    .ToString(MessageLength, 16)
                    .PadLeft(LengthFieldSize * 2, '0')
                    .ToBytes();
                Array.Copy(
                    messageLengthBytes,
                    0,
                    tmp,
                    16 - LengthFieldSize,
                    messageLengthBytes.Length
                );
                return tmp;
            }
        }

        /// <summary>
        /// Get the message authentication code (MAC).
        /// </summary>
        internal byte[] MessageAuthenticationCode
        {
            get
            {
                var macLenght = MessageAuthenticationCodeLength;
                var messageLength = MessageLength;
                var cipherText = CipherText;
                return GetEncryptedKeyStreamBlock(0)
                    .Take(macLenght)
                    .Select((b, i) => (byte)(b ^ cipherText[messageLength + i]))
                    .ToArray();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aesBlockCipher"></param>
        /// <param name="cipherText"></param>
        /// <param name="nonce"></param>
        /// <param name="messageAuthenticationCodeLength"></param>
        public CcmCipherText(
            Aes aesBlockCipher,
            byte[] cipherText,
            byte[] nonce,
            int messageAuthenticationCodeLength
        )
        {
            _aesBlockCipher = aesBlockCipher;
            _cipherText = cipherText;
            _messageAuthenticationCodeLength = messageAuthenticationCodeLength;
            _nonce = nonce;
        }

        /// <summary>
        /// Get the key stream block at the specified index.
        /// </summary>
        /// <param name="index">
        /// The key stream block index.
        /// </param>
        /// <returns>
        /// The key stream block at the specified index.
        /// </returns>
        internal virtual byte[] GetKeyStreamBlock(int index)
        {
            var messageLengthFlagFieldSize = LengthFieldSize;
            var keyStreamBlock = new byte[16];
            string flagBits =
                "00000" +
                Convert
                    .ToString(messageLengthFlagFieldSize - 1, 2)
                    .PadLeft(3, '0');
            keyStreamBlock[0] = Convert.ToByte(flagBits, 2);
            var nonce = Nonce;

            Array.Copy(nonce, 0, keyStreamBlock, 1, nonce.Length);
            return keyStreamBlock;
        }

        /// <summary>
        /// Get the encrypted key stream block at the specified index.
        /// </summary>
        /// <param name="index">
        /// The encrypted key stream block index.
        /// </param>
        /// <returns>
        /// The encrypted key stream block at the specified index.
        /// </returns>
        internal virtual byte[] GetEncryptedKeyStreamBlock(int index)
        {
            var aes = AesBlockCipher;
            var keyStreamBlock = GetKeyStreamBlock(0);
            var encryptor = aes.CreateEncryptor();
            var encryptedKeyStreamBlock = new byte[16];
            encryptor.TransformBlock(
                keyStreamBlock,
                0,
                16,
                encryptedKeyStreamBlock,
                0
            );
            return encryptedKeyStreamBlock;
        }
    }
}
