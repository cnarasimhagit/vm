﻿using System;
using System.IO;
using System.Security.Cryptography;

using CommonServiceLocator;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Unity;
using Unity.ServiceLocation;

namespace vm.Aspects.Security.Cryptography.Ciphers.Tests
{
    [TestClass]
    public class ProtectedKeyCipherTest : GenericCipherTest<ProtectedKeyCipher>
    {
        const string keyFileName = "protected.key";

        static IUnityContainer _container;
        static IServiceLocator _serviceLocator;

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            _container = new UnityContainer();
            _container.RegisterType<SymmetricAlgorithm, TripleDESCryptoServiceProvider>();

            _serviceLocator = new UnityServiceLocator(_container);
            ServiceLocator.SetLocatorProvider(() => _serviceLocator);

            ClassCleanup();
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            var keyManagement = GetCipherImpl() as IKeyManagement;

            if (keyManagement.KeyLocation != null &&
                keyManagement.KeyLocation.EndsWith(keyFileName, StringComparison.InvariantCultureIgnoreCase) &&
                File.Exists(keyManagement.KeyLocation))
                File.Delete(keyManagement.KeyLocation);
        }

        static ICipherAsync GetCipherImpl() => new ProtectedKeyCipher(null, keyFileName);

        public override ICipherAsync GetCipher(bool base64 = false)
        {
            var cipher = GetCipherImpl();

            cipher.Base64Encoded = base64;
            return cipher;
        }

        public override ICipherAsync GetPublicCertCipher(bool base64 = false)
        {
            throw new InvalidOperationException();
        }
    }
}
