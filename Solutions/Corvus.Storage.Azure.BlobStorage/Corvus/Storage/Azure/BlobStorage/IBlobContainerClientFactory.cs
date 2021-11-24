// <copyright file="IBlobContainerClientFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using Azure.Storage.Blobs;

namespace Corvus.Storage.Azure.BlobStorage
{
    /// <summary>
    /// A factory for a <see cref="BlobContainerClient"/>.
    /// </summary>
    public interface IBlobContainerClientFactory : IStorageContextFactory<BlobContainerClient, BlobContainerConfiguration>
    {
    }
}
