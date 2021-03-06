﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace vm.Aspects.Security.Cryptography.Ciphers.Tests
{
    [TestClass]
    public class RsaSignerSha256Test : GenericHasherTest<RsaSigner>
    {
        public override IHasherTasks GetHasher() => new RsaSigner(CertificateFactory.GetSigningSha256Certificate(), Algorithms.Hash.Sha256);     // SHA1 also works with this cert

        public override IHasherTasks GetHasher(int saultLength) => GetHasher();

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

        [TestMethod]
        public void FinalizerTest()
        {
            var target = new WeakReference<RsaSigner>((RsaSigner)GetHasher());

            GC.Collect();


            Assert.IsFalse(target.TryGetTarget(out var collected));
        }
        #endregion
    }
}
