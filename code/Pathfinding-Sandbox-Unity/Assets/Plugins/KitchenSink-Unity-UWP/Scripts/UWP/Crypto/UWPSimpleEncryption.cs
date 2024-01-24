using dk.UWP.Interfaces;

using System;

#if NETFX_CORE

using dk.Crypto;

using System.Runtime.InteropServices.WindowsRuntime;

using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

#endif

namespace dk.UWP.Crypto {
    /// <summary>
    /// UWP implementation of SimpleEncryption.
    /// </summary>
    public class UWPSimpleEncryption : IUWPSimpleEncryptionWrapper {
        #if NETFX_CORE

        private const uint _DEFAULT_ITERATION_COUNT = 10000;

        /// <summary>
        /// 
        /// </summary>
        public string symmetricAlgorithmName {
            get {
                return _symmetricAlgorithmName;
            }
            set {
                _symmetricAlgorithmName = value;
            }
        }

        protected string _symmetricAlgorithmName = SymmetricAlgorithmNames.AesCbcPkcs7;

        /// <summary>
        /// Gets or sets the iteration count to be used for key derivation.
        /// </summary>
        public uint iterationCount {
            get { return _iterationCount; }
            set { _iterationCount = value; }
        }

        protected uint _iterationCount = _DEFAULT_ITERATION_COUNT;

        protected string _keyDerivationAlgorithmName = KeyDerivationAlgorithmNames.Pbkdf2Sha256;
        protected KeyDerivationAlgorithmProvider _keyDerivationAlgorithmProvider = default(KeyDerivationAlgorithmProvider);
        protected SymmetricKeyAlgorithmProvider _symmetricAlgorithmProvider = default(SymmetricKeyAlgorithmProvider);

        #region MonoBehaviours

        /// <summary>
        /// 
        /// </summary>
        public void Awake() {
            _keyDerivationAlgorithmProvider = KeyDerivationAlgorithmProvider.OpenAlgorithm(_keyDerivationAlgorithmName);
            _symmetricAlgorithmProvider = SymmetricKeyAlgorithmProvider.OpenAlgorithm(_symmetricAlgorithmName);
        }

        #endregion

        #endif

        /// <summary>
        /// Encrypt the specified clearData with Key and IV.
        /// </summary>
        /// <param name="clearData">Clear data.</param>
        /// <param name="Key">Key.</param>
        /// <param name="IV">IV.</param>
        public override byte[] Encrypt(byte[] clearData, byte[] Key, byte[] IV) {
            #if NETFX_CORE

            IBuffer dataBuffer = clearData.AsBuffer();
            IBuffer keyBuffer = Key.AsBuffer();
            IBuffer ivBuffer = IV.AsBuffer();
            CryptographicKey key = GetSymmetricKey(keyBuffer);
            IBuffer encryptedData = CryptographicEngine.Encrypt(key, dataBuffer, ivBuffer);

            return encryptedData.ToArray();

            #endif

            throw new NotImplementedException("This feature is not available in non-UWP applications.");
        }

        /// <summary>
        /// Encrypt a string into a string using a password.
        /// Uses Encrypt(byte[], byte[], byte[]).
        /// </summary>
        /// <param name="clearText">Clear text.</param>
        /// <param name="Password">Password.</param>
        public override string Encrypt(string clearText, string Password) {
            #if NETFX_CORE

            EncryptionData data = DeriveKeyAndIVData(Password);
            byte[] clearBytes = System.Text.Encoding.Unicode.GetBytes(clearText);
            byte[] keyBytes = data.key.ToArray();
            byte[] ivBytes = data.iv.ToArray();
            byte[] encryptedData = Encrypt(clearBytes, keyBytes, ivBytes);

            return Convert.ToBase64String(encryptedData);

            #endif

            throw new NotImplementedException("This feature is not available in non-UWP applications.");
        }

        /// <summary>
        /// Encrypt bytes into bytes using a password.
        /// Uses Encrypt(byte[], byte[], byte[]) 
        /// </summary>
        /// <param name="clearData">Clear data.</param>
        /// <param name="Password">Password.</param>
        public override byte[] Encrypt(byte[] clearData, string Password) {
            #if NETFX_CORE

            EncryptionData data = DeriveKeyAndIVData(Password);
            byte[] keyBytes = data.key.ToArray();
            byte[] ivBytes = data.iv.ToArray();
            byte[] encryptedData = Encrypt(clearData, keyBytes, ivBytes);

            return encryptedData;

            #endif

            throw new NotImplementedException("This feature is not available in non-UWP applications.");
        }

        /// <summary>
        /// Encrypt a file into another file using a password.
        /// </summary>
        /// <param name="fileIn">File in.</param>
        /// <param name="fileOut">File out.</param>
        /// <param name="Password">Password.</param>
        public override void Encrypt(string fileIn, string fileOut, string Password) {
            throw new NotImplementedException("This feature is not available in UWP applications.");
        }

        /// <summary>
        /// Decrypt a byte array into a byte array using a key and an IV.
        /// </summary>
        /// <param name="cipherData">Cipher data.</param>
        /// <param name="Key">Key.</param>
        /// <param name="IV">I.</param>
        public override byte[] Decrypt(byte[] cipherData, byte[] Key, byte[] IV) {
            #if NETFX_CORE

            IBuffer cipherDataBuffer = cipherData.AsBuffer();
            IBuffer keyBuffer = Key.AsBuffer();
            IBuffer ivBuffer = IV.AsBuffer();
            CryptographicKey key = GetSymmetricKey(keyBuffer);
            IBuffer decryptedData = CryptographicEngine.Decrypt(key, cipherDataBuffer, ivBuffer);

            return decryptedData.ToArray();

            #endif

            throw new NotImplementedException("This feature is not available in non-UWP applications.");
        }

        /// <summary>
        /// Decrypt a string into a string using a password.
        /// Uses Decrypt(byte[], byte[], byte[]).
        /// </summary>
        /// <param name="cipherText">Cipher text.</param>
        /// <param name="Password">Password.</param>
        public override string Decrypt(string cipherText, string Password) {
            #if NETFX_CORE

            EncryptionData data = DeriveKeyAndIVData(Password);
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            byte[] keyBytes = data.key.ToArray();
            byte[] ivBytes = data.iv.ToArray();
            byte[] decryptedData = Decrypt(cipherBytes, keyBytes, ivBytes);

            return System.Text.Encoding.Unicode.GetString(decryptedData);

            #endif

            throw new NotImplementedException("This feature is not available in non-UWP applications.");
        }

        /// <summary>
        /// Decrypt bytes into bytes using a password.
        /// Uses Decrypt(byte[], byte[], byte[]).
        /// </summary>
        /// <param name="cipherData">Cipher data.</param>
        /// <param name="Password">Password.</param>
        public override byte[] Decrypt(byte[] cipherData, string Password) {
            #if NETFX_CORE

            EncryptionData data = DeriveKeyAndIVData(Password);
            byte[] keyBytes = data.key.ToArray();
            byte[] ivBytes = data.iv.ToArray();
            byte[] decryptedData = Decrypt(cipherData, keyBytes, ivBytes);

            return decryptedData;

            #endif

            throw new NotImplementedException("This feature is not available in non-UWP applications.");
        }

        /// <summary>
        /// Decrypt a file into another file using a password.
        /// </summary>
        /// <param name="fileIn">File in.</param>
        /// <param name="fileOut">File out.</param>
        /// <param name="Password">Password.</param>
        public override void Decrypt(string fileIn, string fileOut, string Password) {
            throw new NotImplementedException("This feature is not available in UWP applications.");
        }

        #region Helper methods

        #if NETFX_CORE

        /// <summary>
        /// Provides a symmetric key for encryption, derived from the provided secret.
        /// </summary>
        /// <param name="secret"></param>
        /// <returns></returns>
        protected CryptographicKey GetSymmetricKey(IBuffer secret) {
            if (string.IsNullOrEmpty(_symmetricAlgorithmName)) _symmetricAlgorithmName = SymmetricAlgorithmNames.AesCbcPkcs7;
            if (_symmetricAlgorithmProvider == null) _symmetricAlgorithmProvider = SymmetricKeyAlgorithmProvider.OpenAlgorithm(_symmetricAlgorithmName);

            if (secret == null || secret.Length == 0) {
                throw new NullReferenceException("IBuffer parameter was null or length was zero.");
            }

            return _symmetricAlgorithmProvider.CreateSymmetricKey(secret);
        }

        /// <summary>
        /// Provides a key and IV in form of IBuffers wrapped in an EncryptionData class.
        /// </summary>
        /// <param name="password"></param>
        /// <returns>An EncryptionData instance.</returns>
        protected EncryptionData DeriveKeyAndIVData(string password) {
            // Create a buffer that contains the secret used during derivation.
            IBuffer secretBuffer = CryptographicBuffer.ConvertStringToBinary(password, BinaryStringEncoding.Utf8);
            IBuffer saltBuffer = SimpleEncryption.saltBytes.AsBuffer();

            // Create the derivation parameters.
            KeyDerivationParameters pbkdf2Params = KeyDerivationParameters.BuildForPbkdf2(saltBuffer, _iterationCount);

            // Derive a key and iv based on the original key and the derivation parameters.
            CryptographicKey derivationKey = _keyDerivationAlgorithmProvider.CreateKey(secretBuffer);
            IBuffer key = CryptographicEngine.DeriveKeyMaterial(derivationKey, pbkdf2Params, 32u);
            IBuffer iv = CryptographicEngine.DeriveKeyMaterial(derivationKey, pbkdf2Params, 16u);

            return new EncryptionData(key, iv);
        }

        #endif

        #endregion

        #if NETFX_CORE

        /// <summary>
        /// Holds fields for a key and IV as IBuffer instances.
        /// </summary>
        protected sealed class EncryptionData {
            public IBuffer key = null;
            public IBuffer iv = null;

            /// <summary>
            /// Initializes this instance with key and IV values.
            /// </summary>
            /// <param name="key"></param>
            /// <param name="iv"></param>
            public EncryptionData(IBuffer key, IBuffer iv) {
                this.key = key;
                this.iv = iv;
            }
        }

        #endif
    }
}
