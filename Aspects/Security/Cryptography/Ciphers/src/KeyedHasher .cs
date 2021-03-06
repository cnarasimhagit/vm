﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

using vm.Aspects.Security.Cryptography.Ciphers.DefaultServices;
using vm.Aspects.Security.Cryptography.Ciphers.Properties;

namespace vm.Aspects.Security.Cryptography.Ciphers
{
    /// <summary>
    /// The class <c>KeyedHasher</c> computes and verifies the cryptographic hash of data for maintaining its integrity.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Crypto package contents:
    ///     <list type="number">
    ///         <item><description>The bytes of the hash.</description></item>
    ///     </list>
    /// </para>
    /// </remarks>
    public class KeyedHasher : IHasherTasks, IKeyManagement, ILightHasher
    {
        #region Fields
        /// <summary>
        /// The public key used for encrypting the hash key.
        /// </summary>
        RSACryptoServiceProvider _publicKey;

        /// <summary>
        /// The private key used for decrypting the hash key.
        /// </summary>
        RSACryptoServiceProvider _privateKey;

        /// <summary>
        /// The underlying hash algorithm.
        /// </summary>
        KeyedHashAlgorithm _hashAlgorithm;
        #endregion

        /// <summary>
        /// The object which is responsible for storing and retrieving the encrypted hash key 
        /// to and from the store with the determined store location name (e.g file I/O).
        /// </summary>
        protected IKeyStorageTasks KeyStorage { get; set; }

        bool IsHashKeyInitialized { get; set; }

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="KeyedHasher" /> class.
        /// </summary>
        /// <param name="certificate">
        /// The certificate containing the public and optionally the private key for encryption and decryption of the hash key.
        /// </param>
        /// <param name="keyLocation">
        /// Seeding name of store location name of the encrypted symmetric key (e.g. relative or absolute path).
        /// Can be <see langword="null" />, empty or whitespace characters only.
        /// The parameter will be passed to the <paramref name="keyLocationStrategy" /> to determine the final store location name path (e.g. relative or absolute path).
        /// </param>
        /// <param name="keyLocationStrategy">
        /// Object which implements the strategy for determining the store location name (e.g. path and filename) of the encrypted symmetric key.
        /// If <see langword="null" /> it defaults to a new instance of the class <see cref="KeyFileLocationStrategy" />.
        /// </param>
        /// <param name="keyStorage">
        /// Object which implements the storing and retrieving of the the encrypted symmetric key to and from the store with the determined location name.
        /// If <see langword="null" /> it defaults to a new instance of the class <see cref="KeyFileStorage" />.
        /// </param>
        /// <param name="hashAlgorithmName">
        /// The keyed hash algorithm name. You can use any of the constants from <see cref="Algorithms.KeyedHash" /> or
        /// <see langword="null" />, empty or whitespace characters only - it will default to <see cref="Algorithms.KeyedHash.Default" />.</param>
        /// <param name="hashAlgorithmFactory">The hash algorithm factory.</param>
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public KeyedHasher(
            X509Certificate2 certificate,
            string keyLocation = null,
            IKeyLocationStrategy keyLocationStrategy = null,
            IKeyStorageTasks keyStorage = null,
            string hashAlgorithmName = Algorithms.KeyedHash.Default,
            IHashAlgorithmFactory hashAlgorithmFactory = null)
            : this(hashAlgorithmName, hashAlgorithmFactory)
        {
            ResolveKeyStorage(keyLocation, keyLocationStrategy, keyStorage);
            InitializeAsymmetricKeys(certificate);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyedHasher"/> class.
        /// </summary>
        protected KeyedHasher(
            string hashAlgorithmName = null,
            IHashAlgorithmFactory hashAlgorithmFactory = null)
        {
            _hashAlgorithm = (KeyedHashAlgorithm)Resolver
                                                    .GetInstanceOrDefault(hashAlgorithmFactory, Resolver.Keyed)
                                                    .Initialize(hashAlgorithmName)
                                                    .Create()
                                                    ;
        }
        #endregion

        #region IHasher Members
        /// <summary>
        /// Gets or sets the length of the salt in bytes. Here it is not used and always returns 0.
        /// </summary>
        /// <value>0</value>
        public virtual int SaltLength
        {
            get { return 0; }
            set { }
        }

        /// <summary>
        /// Computes the hash of a <paramref name="dataStream" /> stream.
        /// </summary>
        /// <param name="dataStream">The data stream.</param>
        /// <returns>
        /// The hash of the stream optionally prepended with the generated salt or <see langword="null"/> if <paramref name="dataStream"/> is <see langword="null"/>.
        /// </returns>
        /// <exception cref="System.ArgumentException">The data stream cannot be read.</exception>
        public virtual byte[] Hash(
            Stream dataStream)
        {
            if (dataStream == null)
                return null;
            if (!dataStream.CanRead)
                throw new ArgumentException(Resources.StreamNotReadable, nameof(dataStream));

            InitializeHashKey();
            using (var hashStream = CreateHashStream(_hashAlgorithm))
            {
                dataStream.CopyTo(hashStream);
                return FinalizeHashing(hashStream);
            }
        }

        /// <summary>
        /// Verifies that the <paramref name="hash" /> of a <paramref name="dataStream" /> is correct.
        /// </summary>
        /// <param name="dataStream">The data stream.</param>
        /// <param name="hash">The hash to verify, optionally prepended with salt.</param>
        /// <returns>
        /// <see langword="true" /> if <paramref name="hash" /> is correct or <paramref name="hash" /> and <paramref name="dataStream"/> are both <see langword="null"/>, 
        /// otherwise <see langword="false" />.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="hash"/> is <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentException">Thrown when the hash has an invalid size.</exception>
        public virtual bool TryVerifyHash(
            Stream dataStream,
            byte[] hash)
        {
            if (dataStream == null)
                return hash == null;

            InitializeHashKey();
            if (hash.Length != _hashAlgorithm.HashSize / 8)
                return false;

            using (var hashStream = CreateHashStream(_hashAlgorithm))
            {
                dataStream.CopyTo(hashStream);

                byte[] computedHash = FinalizeHashing(hashStream);

                return computedHash.ConstantTimeEquals(hash);
            }
        }

        /// <summary>
        /// Computes the hash of a specified <paramref name="data" />.
        /// </summary>
        /// <param name="data">The data to be hashed.</param>
        /// <returns>The hash of the <paramref name="data" /> optionally prepended with the generated salt or <see langword="null" /> if <paramref name="data" /> is <see langword="null" />.
        /// </returns>
        public virtual byte[] Hash(
            byte[] data)
        {
            if (data == null)
                return null;

            InitializeHashKey();
            using (var hashStream = CreateHashStream(_hashAlgorithm))
            {
                hashStream.Write(data, 0, data.Length);
                return FinalizeHashing(hashStream);
            }
        }

        /// <summary>
        /// Verifies the hash of the specified <paramref name="data" />.
        /// </summary>
        /// <param name="data">The data which hash needs to be verified.</param>
        /// <param name="hash">The hash with optionally prepended salt to be verified.</param>
        /// <returns>
        /// <see langword="true" /> if the hash is correct or <paramref name="hash" /> and <paramref name="data"/> are both <see langword="null"/>, otherwise <see langword="false" />.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="hash"/> is <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentException">Thrown when the hash is invalid.</exception>
        public virtual bool TryVerifyHash(
            byte[] data,
            byte[] hash)
        {
            if (data == null)
                return hash == null;

            InitializeHashKey();
            if (hash.Length != _hashAlgorithm.HashSize / 8)
                return false;

            using (var hashStream = CreateHashStream(_hashAlgorithm))
            {
                hashStream.Write(data, 0, data.Length);

                byte[] computedHash = FinalizeHashing(hashStream);

                return computedHash.ConstantTimeEquals(hash);
            }
        }
        #endregion

        #region IHasherAsync members
        /// <summary>
        /// Computes the hash of a <paramref name="dataStream" /> stream.
        /// </summary>
        /// <param name="dataStream">The data stream.</param>
        /// <returns>
        /// The hash of the stream optionally prepended with the generated salt or <see langword="null"/> if <paramref name="dataStream"/> is <see langword="null"/>.
        /// </returns>
        /// <exception cref="System.ArgumentException">The data stream cannot be read.</exception>
        public virtual async Task<byte[]> HashAsync(
            Stream dataStream)
        {
            if (dataStream == null)
                return null;
            if (!dataStream.CanRead)
                throw new ArgumentException(Resources.StreamNotReadable, nameof(dataStream));

            await InitializeHashKeyAsync();
            using (var hashStream = CreateHashStream(_hashAlgorithm))
            {
                await dataStream.CopyToAsync(hashStream);
                return FinalizeHashing(hashStream);
            }
        }

        /// <summary>
        /// Verifies that the <paramref name="hash" /> of a <paramref name="dataStream" /> is correct.
        /// </summary>
        /// <param name="dataStream">The data stream.</param>
        /// <param name="hash">The hash to verify, optionally prepended with salt.</param>
        /// <returns>
        /// <see langword="true" /> if <paramref name="hash" /> is correct or <paramref name="hash" /> and <paramref name="dataStream"/> are both <see langword="null"/>, 
        /// otherwise <see langword="false" />.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="hash"/> is <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentException">Thrown when the hash has an invalid size.</exception>
        public virtual async Task<bool> TryVerifyHashAsync(
            Stream dataStream,
            byte[] hash)
        {
            if (dataStream == null)
                return hash == null;
            if (!dataStream.CanRead)
                throw new ArgumentException(Resources.StreamNotReadable, nameof(dataStream));

            await InitializeHashKeyAsync();
            if (hash.Length != _hashAlgorithm.HashSize / 8)
                return false;

            using (var hashStream = CreateHashStream(_hashAlgorithm))
            {
                await dataStream.CopyToAsync(hashStream);

                byte[] computedHash = FinalizeHashing(hashStream);

                return computedHash.ConstantTimeEquals(hash);
            }
        }
        #endregion

        #region IKeyManagement Members
        /// <summary>
        /// Gets the physical storage location name of a symmetric key, e.g. the path and filename of a file.
        /// </summary>
        public virtual string KeyLocation { get; protected set; }

        /// <summary>
        /// Imports the symmetric key as a clear text.
        /// </summary>
        /// <param name="key">The key.</param>
        public virtual void ImportSymmetricKey(
            byte[] key)
        {
            _hashAlgorithm.Key = key;
            IsHashKeyInitialized = true;
            KeyStorage?.PutKey(EncryptHashKey(), KeyLocation);
        }

        /// <summary>
        /// Exports the symmetric key as a clear text.
        /// </summary>
        /// <returns>Array of bytes of the symmetric key or <see langword="null"/> if the cipher does not have a symmetric key.</returns>
        public virtual byte[] ExportSymmetricKey()
        {
            InitializeHashKey();
            return _hashAlgorithm.Key;
        }

        /// <summary>
        /// Asynchronously imports the symmetric key as a clear text.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the process of asynchronously importing the symmetric key.
        /// </returns>
        public virtual async Task ImportSymmetricKeyAsync(
            byte[] key)
        {
            _hashAlgorithm.Key = key;
            await KeyStorage.PutKeyAsync(EncryptHashKey(), KeyLocation);
        }

        /// <summary>
        /// Asynchronously exports the symmetric key as a clear text.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> object representing the process of asynchronously exporting the symmetric key including the result -
        /// array of bytes of the symmetric key or <see langword="null"/> if the cipher does not have a symmetric key.
        /// </returns>
        public virtual async Task<byte[]> ExportSymmetricKeyAsync()
        {
            await InitializeHashKeyAsync();
            return _hashAlgorithm.Key;
        }
        #endregion

        /// <summary>
        /// Initializes the hash key storage by executing the key location strategy.
        /// </summary>
        /// <param name="keyLocation">The name of the hash key location.</param>
        /// <param name="keyLocationStrategy">The hash key location strategy.</param>
        /// <param name="keyStorage">The hash key storage.</param>
        protected void ResolveKeyStorage(
            string keyLocation,
            IKeyLocationStrategy keyLocationStrategy,
            IKeyStorageTasks keyStorage)
        {
            KeyLocation         = Resolver.GetInstanceOrDefault(keyLocationStrategy).GetKeyLocation(keyLocation);
            KeyStorage          = Resolver.GetInstanceOrDefault(keyStorage);
        }

        void InitializeAsymmetricKeys(
            X509Certificate2 certificate)
        {
            if (certificate == null)
                throw new ArgumentNullException(nameof(certificate));

            _publicKey = (RSACryptoServiceProvider)certificate.PublicKey.Key;

            if (certificate.HasPrivateKey)
                _privateKey = (RSACryptoServiceProvider)certificate.PrivateKey;
        }

        /// <summary>
        /// Gets the keyed hash algorithm implementation.
        /// </summary>
        /// <value>The keyed hash.</value>
        protected virtual KeyedHashAlgorithm KeyedHash => _hashAlgorithm;

        /// <summary>
        /// Initializes the asymmetric key.
        /// </summary>
        protected void InitializeHashKey()
        {
            if (!IsHashKeyInitialized)
            {
                if (KeyStorage.KeyLocationExists(KeyLocation))
                    DecryptHashKey(
                        KeyStorage.GetKey(KeyLocation));
                else
                    KeyStorage.PutKey(EncryptHashKey(), KeyLocation);

                IsHashKeyInitialized = true;
            }

            _hashAlgorithm.Initialize();
        }

        /// <summary>
        /// Asynchronously initializes the asymmetric key.
        /// </summary>
        protected async Task InitializeHashKeyAsync()
        {

            if (!IsHashKeyInitialized)
            {
                if (KeyStorage.KeyLocationExists(KeyLocation))
                    DecryptHashKey(
                        await KeyStorage.GetKeyAsync(KeyLocation));
                else
                    await KeyStorage.PutKeyAsync(EncryptHashKey(), KeyLocation);

                IsHashKeyInitialized = true;
            }

            _hashAlgorithm.Initialize();
        }

        /// <summary>
        /// Encrypts the hash key using the public key.
        /// </summary>
        /// <returns>The key bytes.</returns>
        protected virtual byte[] EncryptHashKey()
        {
            if (_publicKey == null)
                throw new InvalidOperationException("The method is not available on light clones.");

            return _publicKey.Encrypt(_hashAlgorithm.Key, true);
        }

        /// <summary>
        /// Decrypts the hash key using the private key.
        /// </summary>
        /// <param name="encryptedKey">The encrypted key.</param>
        protected virtual void DecryptHashKey(
            byte[] encryptedKey)
        {
            if (_privateKey == null)
                throw new InvalidOperationException(_publicKey == null
                                                        ? "The method is not available on light clones."
                                                        : "The certificate does not contain a private key.");

            _hashAlgorithm.Key = _privateKey.Decrypt(encryptedKey, true);
        }

        /// <summary>
        /// Creates the crypto stream.
        /// </summary>
        /// <returns>CryptoStream.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "It will be disposed by the calling code.")]
        protected virtual CryptoStream CreateHashStream(
            HashAlgorithm hashAlgorithm)
        {
            if (hashAlgorithm == null)
                throw new ArgumentNullException(nameof(hashAlgorithm));

            return new CryptoStream(new NullStream(), hashAlgorithm, CryptoStreamMode.Write);
        }

        /// <summary>
        /// Finalizes the hashing.
        /// </summary>
        /// <param name="hashStream">The hash stream.</param>
        /// <returns>The hash.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="hashStream" /> is <see langword="null" />.</exception>
        protected virtual byte[] FinalizeHashing(
            CryptoStream hashStream)
        {
            if (hashStream == null)
                throw new ArgumentNullException(nameof(hashStream));
            if (!hashStream.CanWrite)
                throw new ArgumentException(Resources.StreamNotWritable, nameof(hashStream));

            if (!hashStream.HasFlushedFinalBlock)
                hashStream.FlushFinalBlock();

            var hash = new byte[_hashAlgorithm.HashSize / 8];

            _hashAlgorithm.Hash.CopyTo(hash, 0);

            return hash;
        }

        #region IDisposable pattern implementation
        /// <summary>
        /// The flag will be set just before the object is disposed.
        /// </summary>
        /// <value>0 - if the object is not disposed yet, any other value - the object is already disposed.</value>
        /// <remarks>
        /// Do not test or manipulate this flag outside of the property <see cref="IsDisposed"/> or the method <see cref="Dispose()"/>.
        /// The type of this field is Int32 so that it can be easily passed to the members of the class <see cref="Interlocked"/>.
        /// </remarks>
        int _disposed;

        /// <summary>
        /// Returns <c>true</c> if the object has already been disposed, otherwise <c>false</c>.
        /// </summary>
        public bool IsDisposed => Interlocked.CompareExchange(ref _disposed, 1, 1) == 1;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <remarks>Invokes the protected virtual <see cref="M:Dispose(true)"/>.</remarks>
        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "It is correct.")]
        public void Dispose()
        {
            // these will be called only if the instance is not disposed and is not in a process of disposing.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs the actual job of disposing the object.
        /// </summary>
        /// <param name="disposing">
        /// Passes the information whether this method is called by <see cref="Dispose()"/> (explicitly or
        /// implicitly at the end of a <c>using</c> statement), or by the <see cref="M:~Hasher"/>.
        /// </param>
        /// <remarks>
        /// If the method is called with <paramref name="disposing"/><c>==true</c>, i.e. from <see cref="Dispose()"/>, 
        /// it will try to release all managed resources (usually aggregated objects which implement <see cref="IDisposable"/> as well) 
        /// and then it will release all unmanaged resources if any. If the parameter is <c>false</c> then 
        /// the method will only try to release the unmanaged resources.
        /// </remarks>
        protected virtual void Dispose(bool disposing)
        {
            // if it is disposed or in a process of disposing - return.
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;

            if (disposing)
                _hashAlgorithm.Dispose();
        }
        #endregion

        #region ILightHasher
        /// <summary>
        /// Releases the associated asymmetric keys. By doing so the instance looses its <see cref="IKeyManagement" /> behavior but the memory footprint becomes much lighter.
        /// The certificate can be dropped only if the underlying symmetric algorithm instance is already initialized.
        /// </summary>
        /// <returns>The hasher.</returns>
        /// <exception cref="InvalidOperationException">
        /// If the underlying hash instance is not initialized yet or if the hashing/hash verification functionality requires asymmetric encryption as well, e.g. signing.
        /// </exception>
        public virtual IHasherTasks ReleaseCertificate()
        {

            if (_publicKey == null)
                return this;

            InitializeHashKey();

            _publicKey.Dispose();
            _publicKey = null;

            _privateKey?.Dispose();
            _privateKey = null;

            KeyStorage = null;

            return this;
        }

        /// <summary>
        /// Creates a new, lightweight clone off of the current hasher and copies certain characteristics, e.g. the hashing key of this instance to it.
        /// </summary>
        /// <returns>The clone.</returns>
        /// <exception cref="InvalidOperationException">
        /// If the underlying hashing algorithm instance is not initialized yet or if the hashing/hash verification functionality requires asymmetric encryption, e.g. signing.
        /// </exception>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The caller owns it.")]
        public virtual IHasherTasks CloneLightHasher()
        {

            InitializeHashKey();

            var hasher = new KeyedHasher
            {
                _hashAlgorithm       = KeyedHashAlgorithm.Create(_hashAlgorithm.GetType().FullName),
                IsHashKeyInitialized = true,
                KeyStorage           = null,
            };

            hasher._hashAlgorithm.Key = (byte[])_hashAlgorithm.Key.Clone();

            return hasher;
        }
        #endregion
    }
}
