﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace vm.Aspects.Security.Cryptography.Ciphers.Test
{
    [TestClass]
    public class RsaSignerTest : GenericHasherTest<RsaSigner>
    {
        public override IHasherAsync GetHasher()
        {
            return new RsaSigner(CertificateFactory.GetSigningCertificate());
        }

        public override IHasherAsync GetHasher(int saultLength)
        {
            return GetHasher();
        }

        #region IsDisposed tests
        [TestMethod]
        public void IsDisposedTest()
        {
            var target = (RsaSigner)GetHasher();

            Assert.IsNotNull(target);

            using (target as IDisposable)
                Assert.IsFalse(target.IsDisposed);
            Assert.IsTrue(target.IsDisposed);

            // should do nothing:
            target.Dispose();
        }

#if NET45
        [TestMethod]
        public void FinalizerTest()
        {
            var target = new WeakReference<RsaSigner>((RsaSigner)GetHasher());

            GC.Collect();

            RsaSigner collected;

            Assert.IsFalse(target.TryGetTarget(out collected));
        } 
#endif
        #endregion

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullCertTest()
        {
            var target = new RsaSigner(null);
        }
    }
}