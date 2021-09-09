// <copyright file="ISingularBlobContainerClientScope.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Storage.Azure.BlobStorage
{
    /// <summary>
    /// A scope (typically application-wide) in which context-specific storage can be retrieved from an
    /// <see cref="IStorageContextFactory{BlobContainerClient, BlobContainerConfiguration}"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Applications that want to use the configuration and storage context caching mechanisms of an
    /// <see cref="IStorageContextFactory{BlobContainerClient, BlobContainerConfiguration}"/>, but
    /// which have no need for multiple scopes (e.g., because the application is not multi-tenanted)
    /// can use dependency injection to demand an implementation of this interface.
    /// </para>
    /// <para>
    /// This adds no methods to the base <see cref="IStorageContextScope{BlobContainerConfiguration}"/>
    /// but it has subtly different semantics: as the "Singular" in the name implies, the expectation
    /// is that there is just one scope (unlike in multi-tenant apps, or other scenarios in which
    /// multiple scopes are in use).
    /// </para>
    /// </remarks>
    public interface ISingularBlobContainerClientScope : IStorageContextScope<BlobContainerConfiguration>
    {
    }
}
