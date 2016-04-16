﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Wyam.Common.IO;
using Wyam.Core.IO;
using Wyam.Testing;

namespace Wyam.Core.Tests.IO
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class FileProviderCollectionTests : BaseFixture
    {
        public class ConstructorTests : FileProviderCollectionTests
        {
            [Test]
            public void SetsDefaultProvider()
            {
                // Given, When
                IFileProvider defaultProvider = Substitute.For<IFileProvider>();
                FileProviderCollection collection = new FileProviderCollection(defaultProvider);

                // Then
                CollectionAssert.AreEquivalent(new Dictionary<string, IFileProvider>
                {
                    { string.Empty, defaultProvider }
                }, collection.Providers);
            }

            [Test]
            public void ThrowsForNullDefaultProvider()
            {
                // Given, When, Then
                Assert.Throws<ArgumentNullException>(() => new FileProviderCollection(null));
            }
        }

        public class AddMethodTests : FileProviderCollectionTests
        {
            [Test]
            public void AddsProvider()
            {
                // Given
                IFileProvider defaultProvider = Substitute.For<IFileProvider>();
                IFileProvider newProvider = Substitute.For<IFileProvider>();
                FileProviderCollection collection = new FileProviderCollection(defaultProvider);

                // When
                bool result = collection.Add("foo", newProvider);

                // Then
                Assert.IsFalse(result);
                CollectionAssert.AreEquivalent(new Dictionary<string, IFileProvider>
                {
                    { string.Empty, defaultProvider },
                    { "foo", newProvider }
                }, collection.Providers);
            }

            [Test]
            public void AddsDuplicateProvider()
            {
                // Given
                IFileProvider defaultProvider = Substitute.For<IFileProvider>();
                IFileProvider oldProvider = Substitute.For<IFileProvider>();
                IFileProvider newProvider = Substitute.For<IFileProvider>();
                FileProviderCollection collection = new FileProviderCollection(defaultProvider);

                // When
                collection.Add("foo", oldProvider);
                bool result = collection.Add("foo", newProvider);

                // Then
                Assert.IsTrue(result);
                CollectionAssert.AreEquivalent(new Dictionary<string, IFileProvider>
                {
                    { string.Empty, defaultProvider },
                    { "foo", newProvider }
                }, collection.Providers);
            }

            [Test]
            public void AddsDefaultProvider()
            {
                // Given
                IFileProvider defaultProvider = Substitute.For<IFileProvider>();
                IFileProvider newProvider = Substitute.For<IFileProvider>();
                FileProviderCollection collection = new FileProviderCollection(defaultProvider);

                // When
                bool result = collection.Add(string.Empty, newProvider);

                // Then
                Assert.IsTrue(result);
                CollectionAssert.AreEquivalent(new Dictionary<string, IFileProvider>
                {
                    { string.Empty, newProvider }
                }, collection.Providers);
            }

            [Test]
            public void ThrowsForNullName()
            {
                // Given
                IFileProvider defaultProvider = Substitute.For<IFileProvider>();
                IFileProvider newProvider = Substitute.For<IFileProvider>();
                FileProviderCollection collection = new FileProviderCollection(defaultProvider);

                // When, Then
                Assert.Throws<ArgumentNullException>(() => collection.Add(null, newProvider));
            }

            [Test]
            public void ThrowsForNullProvider()
            {
                // Given
                IFileProvider defaultProvider = Substitute.For<IFileProvider>();
                FileProviderCollection collection = new FileProviderCollection(defaultProvider);

                // When, Then
                Assert.Throws<ArgumentNullException>(() => collection.Add("foo", null));
            }
        }

        public class RemoveMethodTests : FileProviderCollectionTests
        {
            [Test]
            public void RemovesExistingProvider()
            {
                // Given
                IFileProvider defaultProvider = Substitute.For<IFileProvider>();
                IFileProvider newProvider = Substitute.For<IFileProvider>();
                FileProviderCollection collection = new FileProviderCollection(defaultProvider);
                collection.Add("foo", newProvider);

                // When
                bool result = collection.Remove("foo");

                // Then
                Assert.IsTrue(result);
                CollectionAssert.AreEquivalent(new Dictionary<string, IFileProvider>
                {
                    { string.Empty, defaultProvider }
                }, collection.Providers);
            }

            [Test]
            public void ReturnsFalseForNonExistingProvider()
            {
                // Given
                IFileProvider defaultProvider = Substitute.For<IFileProvider>();
                FileProviderCollection collection = new FileProviderCollection(defaultProvider);

                // When
                bool result = collection.Remove("foo");

                // Then
                Assert.IsFalse(result);
                CollectionAssert.AreEquivalent(new Dictionary<string, IFileProvider>
                {
                    { string.Empty, defaultProvider }
                }, collection.Providers);
            }

            [Test]
            public void ThrowsForNullName()
            {
                // Given
                IFileProvider defaultProvider = Substitute.For<IFileProvider>();
                IFileProvider newProvider = Substitute.For<IFileProvider>();
                FileProviderCollection collection = new FileProviderCollection(defaultProvider);
                collection.Add("foo", newProvider);

                // When, Then
                Assert.Throws<ArgumentNullException>(() => collection.Remove(null));
            }

            [Test]
            public void ThrowsForDefaultProvider()
            {
                // Given
                IFileProvider defaultProvider = Substitute.For<IFileProvider>();
                FileProviderCollection collection = new FileProviderCollection(defaultProvider);

                // When, Then
                Assert.Throws<ArgumentException>(() => collection.Remove(string.Empty));
            }
        }

        public class GetMethodTests : FileProviderCollectionTests
        {
            [Test]
            public void ReturnsProvider()
            {
                // Given
                IFileProvider defaultProvider = Substitute.For<IFileProvider>();
                IFileProvider newProvider = Substitute.For<IFileProvider>();
                FileProviderCollection collection = new FileProviderCollection(defaultProvider);
                collection.Add("foo", newProvider);

                // When
                IFileProvider result = collection.Get("foo");

                // Then
                Assert.AreEqual(newProvider, result);
            }

            [Test]
            public void ReturnsDefaultProvider()
            {
                // Given
                IFileProvider defaultProvider = Substitute.For<IFileProvider>();
                IFileProvider newProvider = Substitute.For<IFileProvider>();
                FileProviderCollection collection = new FileProviderCollection(defaultProvider);
                collection.Add("foo", newProvider);

                // When
                IFileProvider result = collection.Get(string.Empty);

                // Then
                Assert.AreEqual(defaultProvider, result);
            }

            [Test]
            public void ThrowsForNullName()
            {
                // Given
                IFileProvider defaultProvider = Substitute.For<IFileProvider>();
                FileProviderCollection collection = new FileProviderCollection(defaultProvider);

                // When, Then
                Assert.Throws<ArgumentNullException>(() => collection.Get(null));
            }

            [Test]
            public void ThrowsForNotFound()
            {
                // Given
                IFileProvider defaultProvider = Substitute.For<IFileProvider>();
                FileProviderCollection collection = new FileProviderCollection(defaultProvider);

                // When, Then
                Assert.Throws<KeyNotFoundException>(() => collection.Get("foo"));
            }
        }

        public class TryGetMethodTests : FileProviderCollectionTests
        {
            [Test]
            public void ReturnsTrueForProvider()
            {
                // Given
                IFileProvider defaultProvider = Substitute.For<IFileProvider>();
                IFileProvider newProvider = Substitute.For<IFileProvider>();
                FileProviderCollection collection = new FileProviderCollection(defaultProvider);
                collection.Add("foo", newProvider);

                // When
                IFileProvider providerResult;
                bool result = collection.TryGet("foo", out providerResult);

                // Then
                Assert.AreEqual(newProvider, providerResult);
                Assert.IsTrue(result);
            }

            [Test]
            public void ReturnsTrueForDefaultProvider()
            {
                // Given
                IFileProvider defaultProvider = Substitute.For<IFileProvider>();
                IFileProvider newProvider = Substitute.For<IFileProvider>();
                FileProviderCollection collection = new FileProviderCollection(defaultProvider);
                collection.Add("foo", newProvider);

                // When
                IFileProvider providerResult;
                bool result = collection.TryGet(string.Empty, out providerResult);

                // Then
                Assert.AreEqual(defaultProvider, providerResult);
                Assert.IsTrue(result);
            }

            [Test]
            public void ReturnsFalseIfNotFound()
            {
                // Given
                IFileProvider defaultProvider = Substitute.For<IFileProvider>();
                FileProviderCollection collection = new FileProviderCollection(defaultProvider);

                // When
                IFileProvider providerResult;
                bool result = collection.TryGet("foo", out providerResult);

                // Then
                Assert.IsFalse(result);
            }

            [Test]
            public void ThrowsForNullName()
            {
                // Given
                IFileProvider defaultProvider = Substitute.For<IFileProvider>();
                FileProviderCollection collection = new FileProviderCollection(defaultProvider);

                // When, Then
                IFileProvider providerResult;
                Assert.Throws<ArgumentNullException>(() => collection.TryGet(null, out providerResult));
            }
        }
    }
}
