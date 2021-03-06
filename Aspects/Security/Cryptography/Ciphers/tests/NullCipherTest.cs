﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace vm.Aspects.Security.Cryptography.Ciphers.Tests
{
    [TestClass]
    public class NullCipherTest : GenericCipherTest<NullCipher>
    {
        public override ICipherTasks GetCipher(bool base64 = false)
            => new NullCipher
            {
                Base64Encoded = base64,
            };

        public override ICipherTasks GetPublicCertCipher(bool base64 = false)
        {
            throw new InvalidOperationException();
        }
    }
}
